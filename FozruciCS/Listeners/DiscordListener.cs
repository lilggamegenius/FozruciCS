using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FozruciCS.Config;
using FozruciCS.Links;
using FozruciCS.Utils;
using LogLevel = DSharpPlus.LogLevel;

namespace FozruciCS.Listeners{
	public class DiscordListener : IListener{
		private static readonly ILog Logger = LogManager.GetLogger<DiscordListener>();
		private readonly DiscordClient _client;
		public Configuration Config=>Program.Config;

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
			AppDomain.CurrentDomain.ProcessExit += ExitHandler;
			Task.Run(async () =>{
				try{ await _client.ConnectAsync(); } catch(Exception e){
					Logger.Error(e);
				}
			});
		}

		public async Task<bool> CommandHandler(MessageCreateEventArgs e){
			if(e.Author == e.Client.CurrentUser){ return false; }
			LinkedDiscordChannel channel = e.Channel;

			string[] args = e.Message.Content.Split(' ');
			if(args[0].StartsWith(e.Client.CurrentUser.Mention)      ||
			   args[0].StartsWith(e.Guild.CurrentMember.DisplayName) ||
			   args[0].StartsWith(e.Client.CurrentUser.Username)){
				string command;
				if(Program.Commands.ContainsCommand(command = args[1].ToLower())){
					ArraySegment<string> segment = new ArraySegment<string>(args, 2, args.Length - 2);
					try{
						await Program.Commands[command].HandleCommand(this, (LinkedDiscordChannel)e.Channel, segment, (LinkedDiscordMessage)e);
					} catch(Exception ex){
						Logger.Error($"Problem processing command: \n{ex}");
						await channel.respond($"Sorry there was a problem processing the command: {ex.Message}");
						return false;
					}

					return true;
				}
			}

			return false;
		}

		private async Task OnNewMessage(MessageCreateEventArgs e){
			StringBuilder builder = new StringBuilder();
			foreach(DiscordAttachment result in e.Message.Attachments){
				if(builder.Length != 0){
					builder.Append(", ");
				}

				builder.Append(result.Url);
			}
			bool isCommand = await CommandHandler(e);
			Logger.InfoFormat($"{(isCommand ? "Command" : "Message")} from ({e.Guild.Name}) #{e.Channel.Name} by {e.Author.GetHostMask()}: {e.Message.Content} {builder}");
		}

		private async Task OnClientOnReady(ReadyEventArgs args){
			Logger.Info("Discord listener is now ready");
		}

		public async void ExitHandler(object sender, EventArgs args){
			await _client.DisconnectAsync();

		}

		private async Task OnClientError(ClientErrorEventArgs e){
			Logger.Error(e.EventName, e.Exception);
			if(e.Exception is AggregateException exception){
				for(int i = 0; i < exception.InnerExceptions.Count; i++){
					Exception innerException = exception.InnerExceptions[i];
					Logger.Error($"[{i + 1}] {innerException.Message}: \n{innerException.StackTrace}", innerException);
				}
			}
		}

		private async Task OnSocketError(SocketErrorEventArgs e)=>Logger.Error(e.Exception.Message, e.Exception);
	}
}
