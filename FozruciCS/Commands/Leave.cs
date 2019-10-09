using System.Collections.Generic;
using System.Threading.Tasks;
using ChatSharp;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NLog;
using NMaier.GetOptNet;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.BotOwner)]
	public class Leave : ICommand{
		internal const string Usage = "Usage: leave <channel>";
		internal const string Epilogue = "This command has no options";

		private const string LeaveMessage = "Leaving";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static Leave(){Program.RegisterCommand(nameof(Leave), new Leave());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			if(!e.server.isIrc){
				await respondTo.respond("This command only works in IRC", e.author);
				return;
			}

			IrcClient client = ((IrcListener)listener).IrcClient;
			LeaveOptions opts = new LeaveOptions();
			try{
				opts.Parse(args);
				if(opts.Parameters.Count != 0){
					foreach(string channelToLeave in opts.Parameters){
						IrcChannel ircChannel;
						if((ircChannel = client.Channels[channelToLeave]) != default){ ircChannel.Part(LeaveMessage); }
					}

					return;
				}

				((LinkedIrcChannel)e.channel).channel.Part(LeaveMessage);
			} catch(GetOptException){ await Help(listener, respondTo, args, e); }
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new LeaveOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Leave.Usage, UsageEpilog = Leave.Epilogue)]
	public class LeaveOptions : GetOpt{
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();
	}
}
