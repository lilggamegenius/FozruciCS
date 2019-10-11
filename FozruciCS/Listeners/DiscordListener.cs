using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FozruciCS.Config;
using FozruciCS.Links;
using FozruciCS.Utils;
using NLog;
using LogLevel = DSharpPlus.LogLevel;
using Timer = System.Timers.Timer;

namespace FozruciCS.Listeners{
	public class DiscordListener : IListener{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		public static Timer titanusTimer = new Timer{Interval = 10 * 1000, AutoReset = true, Enabled = true};
		private readonly DiscordClient _client;

		public Dictionary<string, List<LinkedMessage>> LoggedMessages = new Dictionary<string, List<LinkedMessage>>();

		public bool titanusDown;

		public DiscordListener(){
			_client = Config.DiscordSocketClient = new DiscordClient(new DiscordConfiguration{
				Token = Config.DiscordToken,
				LogLevel = LogLevel.Debug
			});
			_client.DebugLogger.LogMessageReceived += (sender, args)=>{
				switch(args.Level){
					case LogLevel.Debug:
						Logger.Debug(args.Message);
						break;
					case LogLevel.Info:
						Logger.Info(args.Message);
						break;
					case LogLevel.Warning:
						Logger.Warn(args.Message);
						break;
					case LogLevel.Error:
						Logger.Error(args.Message);
						break;
					case LogLevel.Critical:
						Logger.Fatal(args.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
			_client.MessageCreated += OnNewMessage;
			_client.Ready += OnClientOnReady;
			_client.ClientErrored += OnClientError;
			_client.SocketErrored += OnSocketError;
			Task.Run(async ()=>{
				try{ await _client.ConnectAsync(); } catch(Exception e){ Logger.Error(e); }
			});
			titanusTimer.Elapsed += async (sender, args)=>{
				DiscordGuild guild = _client.Guilds[215943758527594496];
				DiscordMember titanus = await guild.GetMemberAsync(376087025234870272);
				//DiscordMember titanus = await guild.GetMemberAsync(131494148350935040); // Test
				if(titanus.Presence == null){
					return; // Discord sometimes doesn't return the Presence so just return if null
				}

				if(titanus.Presence.Status == UserStatus.Offline){
					if(!titanusDown){
						await guild?.GetChannel(215943758527594496)?.SendMessageAsync("<@206237974105554944> Titanus is down");
						//await guild?.GetChannel(215943758527594496)?.SendMessageAsync("Down message"); // Test
						titanusDown = true;
					}
				} else{ titanusDown = false; }
			};
		}
		public static Configuration Config=>Program.Config;

		public async void ExitHandler(object sender, EventArgs args){await _client.DisconnectAsync();}
		public void LogMessage(LinkedChannel channel, LinkedMessage message){
			string channelName = channel.name;
			if(!LoggedMessages.ContainsKey(channelName)){ LoggedMessages[channelName] = new List<LinkedMessage>(); }

			LoggedMessages[channelName].Add(message);
		}
		public async Task<List<LinkedMessage>> GetMessages(LinkedChannel channel){
			string channelName = channel.name;
			if(LoggedMessages.ContainsKey(channelName)){ return LoggedMessages[channelName]; }

			bool lockTaken = false;
			try{
				Monitor.TryEnter(_client, TimeSpan.Zero, ref lockTaken);
				if(lockTaken){
					List<LinkedMessage> messages = new List<LinkedMessage>();
					if(channel is LinkedDiscordChannel discordChannel){
						IReadOnlyList<DiscordMessage> discordMessages = await discordChannel.channel.GetMessagesAsync();
						for(int i = discordMessages.Count - 1; i >= 0; i--){
							DiscordMessage discordMessage = discordMessages[i];
							if(discordMessage.MessageType != MessageType.Default){ continue; }

							if(discordMessage.WebhookMessage){ continue; }

							messages.Add((LinkedDiscordMessage)discordMessage);
						}

						LoggedMessages[discordChannel.name] = messages;
					}
				}
			} finally{
				if(lockTaken){ Monitor.Exit(_client); }
			}

			return LoggedMessages[channelName];
		}

		public async Task<bool> CommandHandler(MessageCreateEventArgs e){
			if(e.Author == e.Client.CurrentUser){ return false; }

			string message = e.Message.Content;
			string[] args = message.SplitMessage();
			IRespondable respondTo;
			if(e.Channel != null){ respondTo = (LinkedDiscordChannel)e.Channel; } else{ respondTo = (LinkedDiscordUser)e.Author; }

			bool commandByName = args[0].StartsWith(e.Client.CurrentUser.Mention)      ||
								 args[0].StartsWith(e.Guild.CurrentMember.DisplayName) ||
								 args[0].StartsWith(e.Client.CurrentUser.Username);
			bool commandByPrefix = message.StartsWithAny(Config.CommandPrefixes);
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
						if(message.StartsWith(prefix)){
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
					LinkedDiscordMessage linkedMessage = e;
					if(!LilGUtil.CheckPermission(command, linkedMessage)){
						await respondTo.respond($"Sorry, you don't have the permission to run {command}");
						return true;
					}

					ArraySegment<string> segment = new ArraySegment<string>(args, offset, args.Length - offset);
					try{ await Program.CommandList[command].HandleCommand(this, respondTo, segment, (LinkedDiscordMessage)e); } catch(Exception ex){
						Logger.Error(ex, "Problem processing command: \n{0}", ex);
						await respondTo.respond($"Sorry there was a problem processing the command: {ex.Message}");
						return true;
					}

					return true;
				}
			}

			return false;
		}

		private async Task OnNewMessage(MessageCreateEventArgs e){
			StringBuilder builder = new StringBuilder();
			foreach(DiscordAttachment result in e.Message.Attachments){
				if(builder.Length != 0){ builder.Append(", "); }

				builder.Append(result.Url);
			}

			bool isCommand = await CommandHandler(e);
			if(!isCommand){ await Program.CommandList.message(this, (LinkedDiscordChannel)e.Channel, (LinkedDiscordMessage)e); }

			Logger.Info("{0} from ({1}) #{2} by {3}: {4} {5}",
						isCommand ? "Command" : "Message",
						e.Guild.Name,
						e.Channel.Name,
						e.Author.GetHostMask(),
						e.Message.Content,
						builder);
			LogMessage((LinkedDiscordChannel)e.Channel, (LinkedDiscordMessage)e);
		}

		private async Task OnClientOnReady(ReadyEventArgs args)=>Logger.Info("Discord listener is now ready");

		private async Task OnClientError(ClientErrorEventArgs e){
			await Task.Run(()=>{
				Logger.Error(e.Exception, e.EventName);
				if(e.Exception is AggregateException exception){
					for(int i = 0; i < exception.InnerExceptions.Count; i++){
						Exception innerException = exception.InnerExceptions[i];
						Logger.Error(innerException, "[{0}] {1}\n{2}", i, innerException.Message, innerException.StackTrace);
					}
				}
			});
		}

		private async Task OnSocketError(SocketErrorEventArgs e)=>await Task.Run(()=>Logger.Error(e.Exception, e.Exception.Message));
	}
}
