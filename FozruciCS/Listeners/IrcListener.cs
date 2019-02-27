using System;
using System.Threading.Tasks;
using ChatSharp;
using ChatSharp.Events;
using Common.Logging;
using FozruciCS.Commands;
using FozruciCS.Config;
using FozruciCS.Links;
using FozruciCS.Utils;

namespace FozruciCS.Listeners{
	public class IrcListener : IListener{
		private static readonly ILog Logger = LogManager.GetLogger<IrcListener>();
		public Configuration.ServerConfiguration Config;
		public IrcClient IrcClient;
		public IrcUser IrcSelf;
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
			AppDomain.CurrentDomain.ProcessExit += ExitHandler;
		}

		public void ExitHandler(object sender, EventArgs args){IrcClient.Quit("Shutting down");}

		public async Task<bool> CommandHandler(PrivateMessageEventArgs e){
			if(e.PrivateMessage.User.Hostmask == IrcSelf.Hostmask){ return false; }

			string message = e.PrivateMessage.Message;
			string messageLower = message.ToLower();
			string[] args = message.Split(' ');
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

				if(Program.Commands.ContainsCommand(command)){
					LinkedIrcMessage linkedMessage = e;
					if(!LilGUtil.CheckPermission(command, linkedMessage.server, linkedMessage.channel, linkedMessage.author)){
						await respondTo.respond($"Sorry, you don't have the permission to run {command}");
						return true;
					}

					ICommand icommand = Program.Commands[command];
					ArraySegment<string> segment = new ArraySegment<string>(args, offset, args.Length - offset);
					try{ await icommand.HandleCommand(this, respondTo, segment, (LinkedIrcMessage)e); } catch(Exception ex){
						Logger.Error($"Problem processing command: \n{ex}");
						await respondTo.respond($"Sorry there was a problem processing the command: {ex.Message}");
						return false;
					}

					return true;
				}
			}

			return false;
		}
		private async void OnChannelMessageRecieved(PrivateMessageEventArgs e){
			bool isCommand = await CommandHandler(e);
			Logger.Info($"{(isCommand ? "Command" : "Message")} from {e.PrivateMessage.Source} by {e.PrivateMessage.User.Hostmask}: {e.PrivateMessage.Message}");
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
			Logger.Info($"{(isCommand ? "Command" : "Message")} from {e.PrivateMessage.User.Hostmask}: {e.PrivateMessage.Message}");
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

			Logger.InfoFormat("Received CTCP message: {0}", message);
		}

		private void OnUserJoinedChannel(object sender, ChannelUserEventArgs e){Logger.InfoFormat("User {0} Joined channel {1}", e.User.Hostmask, e.Channel.Name);}

		private void OnRawMessageRecieved(object sender, RawMessageEventArgs args){Logger.DebugFormat("<<< {0}", args.Message);}

		private void OnRawMessageSent(object sender, RawMessageEventArgs args){Logger.DebugFormat(">>> {0}", args.Message);}

		private void OnConnect(object sender, EventArgs e){
			Task.Run(()=>{
				foreach(string channel in Config.channelOptions.Keys){
					string[] channelValues = channel.Split(new[]{' '}, 1);
					IrcClient.JoinChannel(channelValues[0]);
				}

				IrcClient.SendRawMessage("ns identify {0} {1}", Config.nickservAccountName, Config.nickservPassword);
			});
		}

		private void OnError(object sender, ErrorEventArgs e){Logger.Error(e.Error.Message, e.Error);}

		private enum CtcpCommands{ PING, FINGER, VERSION, USERINFO, CLIENTINFO, SOURCE, TIME, PAGE, AVATAR }
	}
}
