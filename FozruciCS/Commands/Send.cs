using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using DSharpPlus.Entities;
using FozruciCS.Links;
using FozruciCS.Listeners;
using FozruciCS.Utils;
using NMaier.GetOptNet;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.BotOwner)]
	public class Send : ICommand{
		internal const string Usage = "Usage: Send <channel>";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<Send>();
		static Send(){Program.RegisterCommand(nameof(Send), new Send());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			SendOptions opts = new SendOptions();
			try{
				opts.Parse(args);
				string msg = opts.Parameters.Count > 0 ? LilGUtil.ArgJoiner(opts.Parameters.ToArray()) : opts.Parameters[0];
				if(opts.Raw){
					LinkedIrcMessage message = (LinkedIrcMessage)e;
					message.Client.SendRawMessage(msg);
					return;
				}

				if(opts.channelId != 0){
					DiscordChannel channel = await Program.Config.DiscordSocketClient.GetChannelAsync(opts.channelId);
					await channel.SendMessageAsync(msg);
					return;
				}

				if(opts.channel != null){
					LinkedIrcMessage message = (LinkedIrcMessage)e;
					if(message.Client.Channels.Contains(opts.channel)){ message.Client.SendMessage(msg, opts.channel); }
				}
			} catch(GetOptException){ await Help(listener, respondTo, args, e); }
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new SendOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Send.Usage, UsageEpilog = Send.Epilogue)]
	public class SendOptions : GetOpt{
		private string _channel;

		public ulong channelId;
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();

		[Argument, FlagArgument(true), ShortArgument('r')] public bool Raw = false;
		[Argument, ShortArgument('c')]
		public string channel{
			get=>_channel;
			// ReSharper disable once UnusedMember.Global
			set{
				string val = value;
				if(val[0] == '#'){
					_channel = val;
					return;
				}

				if((val.Length == 18) &&
				   ulong.TryParse(val, out ulong id)){
					_channel = "#" + Program.Config.DiscordSocketClient.GetChannelAsync(id).Result.Name;
					channelId = id;
					return;
				}

				throw new InvalidValueException("Value for channel must be a valid irc channel or discord ID");
			}
		}
	}
}
