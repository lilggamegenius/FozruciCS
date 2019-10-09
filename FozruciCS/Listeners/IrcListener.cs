using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatSharp;
using ChatSharp.Events;
using FozruciCS.Commands;
using FozruciCS.Config;
using FozruciCS.Links;
using FozruciCS.Utils;
using NLog;

namespace FozruciCS.Listeners{
	public class IrcListener : IListener{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		public Configuration.ServerConfiguration Config;
		public IrcClient IrcClient;
		public IrcUser IrcSelf;
		public Dictionary<LinkedChannel, List<LinkedMessage>> LoggedMessages = new Dictionary<LinkedChannel, List<LinkedMessage>>();
		public IrcListener(Configuration.ServerConfiguration configServer){
			Config = configServer;
			IrcSelf = Config.IrcSelf = new IrcUser(null, Config.NickName, Config.UserName, Config.serverPassword, Config.RealName);
			IrcClient = Config.IrcClient = new IrcClient($"{Config.server}:{Config.port}", IrcSelf, Config.SSL);
			IrcClient.IgnoreInvalidSSL = Config.IgnoreInvalidSSL;
			IrcClient.RawMessageRecieved += OnRawMessageRecieved;
			IrcClient.RawMessageSent += OnRawMessageSent;
			IrcClient.ConnectionComplete += OnConnect;
			IrcClient.UserJoinedChannel += OnUserJoinedChannel;
			IrcClient.PrivateMessageRecieved += OnPrivateMessageRecieved;
			//ircClient.ChannelMessageRecieved += onChannelMessageRecieved;
			IrcClient.Error += OnError;
			IrcClient.ConnectAsync();
		}

		public void ExitHandler(object sender, EventArgs args){IrcClient.Quit("Shutting down");}
		public void LogMessage(LinkedChannel channel, LinkedMessage message){
			if(!LoggedMessages.ContainsKey(channel)){ LoggedMessages.Add(channel, new List<LinkedMessage>()); }

			LoggedMessages[channel].Add(message);
		}
		public List<LinkedMessage> GetMessages(LinkedChannel channel){
			if(!LoggedMessages.ContainsKey(channel)){ return new List<LinkedMessage>(); }

			return LoggedMessages[channel];
		}

		public async Task<bool> CommandHandler(PrivateMessageEventArgs e){
			if(e.PrivateMessage.User.Hostmask == IrcSelf.Hostmask){ return false; }

			string message = e.PrivateMessage.Message;
			string messageLower = message.ToLower();
			string[] args = message.SplitMessage();
			IRespondable respondTo;
			if(e.PrivateMessage.IsChannelMessage){ respondTo = (LinkedIrcChannel)e.PrivateMessage.Channel; } else{ respondTo = (LinkedIrcUser)e.PrivateMessage.User; }

			string first = args[0].ToLower();
			bool commandByName = first.StartsWith(IrcSelf.Nick.ToLower());
			bool commandByPrefix = messageLower.StartsWithAny(Config.CommandPrefixes);
			if(commandByName || commandByPrefix){
				string command = null;
				int offset = 1;
				if(commandByName){
					if(args.Length < 2){ return false; }

					command = args[1];
					offset = 2;
				}

				if(commandByPrefix){
					foreach(string prefix in Config.CommandPrefixes){
						if(messageLower.StartsWith(prefix)){
							if(prefix.EndsWith(" ")){
								if(args.Length < 2){ return false; }

								command = args[1];
								offset = 2;
								break;
							}

							command = args[0].Substring(prefix.Length);
							break;
						}
					}
				}

				if(command == null){ return false; }

				if(Program.CommandList.ContainsCommand(command)){
					LinkedIrcMessage linkedMessage = e;
					if(!LilGUtil.CheckPermission(command, linkedMessage)){
						await respondTo.respond($"Sorry, you don't have the permission to run {command}");
						return true;
					}

					ICommand icommand = Program.CommandList[command];
					ArraySegment<string> segment = new ArraySegment<string>(args, offset, args.Length - offset);
					try{ await icommand.HandleCommand(this, respondTo, segment, (LinkedIrcMessage)e); } catch(Exception ex){
						Logger.Error(ex, "Problem processing command: \n{0}", ex.StackTrace);
						await respondTo.respond($"Sorry there was a problem processing the command: {ex.Message}");
						return true;
					}

					return true;
				}
			}

			return false;
		}
		private async void OnChannelMessageRecieved(PrivateMessageEventArgs e){
			bool isCommand = await CommandHandler(e);
			if(!isCommand){ await Program.CommandList.message(this, (LinkedIrcChannel)e.PrivateMessage.Channel, (LinkedIrcMessage)e); }

			Logger.Info("{0} from {1} by {2}: {3}",
						isCommand ? "Command" : "Message",
						e.PrivateMessage.Source,
						e.PrivateMessage.User.Hostmask,
						e.PrivateMessage.Message);
		}

		private async void OnPrivateMessageRecieved(object sender, PrivateMessageEventArgs e){
			if(e.PrivateMessage.Message.IsCtcp()){
				OnCtcpMessageRecieved(e);
				return;
			}

			if(e.PrivateMessage.IsChannelMessage){
				OnChannelMessageRecieved(e);
				return;
			}

			bool isCommand = await CommandHandler(e);
			if(!isCommand){ await Program.CommandList.message(this, (LinkedIrcUser)e.PrivateMessage.User, (LinkedIrcMessage)e); }

			Logger.Info("{0} from {1}: {2}", isCommand ? "Command" : "Message", e.PrivateMessage.User.Hostmask, e.PrivateMessage.Message);
		}

		private void OnCtcpMessageRecieved(PrivateMessageEventArgs e){
			string message = e.PrivateMessage.Message.Substring(1, e.PrivateMessage.Message.Length - 2);
			string command = message.SplitMessage()[0].Trim();
			switch(command){
				case nameof(CtcpCommands.PING):
					IrcClient.SendNotice("PONG".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.FINGER):
					IrcClient.SendNotice($"{command} You ought to be arrested for fingering a bot!".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.VERSION):
					IrcClient.SendNotice($"{command} {Program.Version}".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.USERINFO):
					goto case nameof(CtcpCommands.VERSION); //ircClient.SendNotice("".ToCTCP());
				//break;
				case nameof(CtcpCommands.CLIENTINFO):
					IrcClient.SendNotice($"{command} ".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.SOURCE):
					IrcClient.SendNotice($"{command} FozruciCS - https://github.com/lilggamegenius/FozruciCS".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.TIME):
					IrcClient.SendNotice($"{command} ".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.PAGE):
					IrcClient.SendNotice($"{command} ".ToCtcp(), e.PrivateMessage.Source);
					break;
				case nameof(CtcpCommands.AVATAR):
					IrcClient.SendNotice($"{command} ".ToCtcp(), e.PrivateMessage.Source);
					break;
			}

			Logger.Info("Received CTCP message: {0}", message);
		}

		private void OnUserJoinedChannel(object sender, ChannelUserEventArgs e){Logger.Info("User {0} Joined channel {1}", e.User.Hostmask, e.Channel.Name);}

		private void OnRawMessageRecieved(object sender, RawMessageEventArgs args){Logger.Debug("<<< {0}", args.Message);}

		private void OnRawMessageSent(object sender, RawMessageEventArgs args){Logger.Debug(">>> {0}", args.Message);}

		private void OnConnect(object sender, EventArgs e){
			Task.Run(()=>{
				foreach(string channel in Config.channelOptions.Keys){
					string[] channelValues = channel.Split(new[]{' '}, 1);
					IrcClient.JoinChannel(channelValues[0]);
				}

				IrcClient.SendRawMessage("ns identify {0} {1}", Config.nickservAccountName, Config.nickservPassword);
			});
		}

		private void OnError(object sender, ErrorEventArgs e){Logger.Error(e.Error, e.Error.Message);}

		private enum CtcpCommands{ PING, FINGER, VERSION, USERINFO, CLIENTINFO, SOURCE, TIME, PAGE, AVATAR }
	}
}
