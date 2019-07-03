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
	public class Join : ICommand{
		internal const string Usage = "Usage: join <channel>";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static Join(){Program.RegisterCommand(nameof(Join), new Join());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			if(!e.server.isIrc){
				await respondTo.respond("This command only works in IRC", e.author);
				return;
			}

			IrcClient client = ((IrcListener)listener).IrcClient;
			JoinOptions opts = new JoinOptions();
			try{
				opts.Parse(args);
				foreach(string channelToJoin in opts.Parameters){ client.Channels.Join(channelToJoin); }
			} catch(GetOptException){ await Help(listener, respondTo, args, e); }
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new JoinOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Join.Usage, UsageEpilog = Join.Epilogue)]
	public class JoinOptions : GetOpt{
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();
	}
}
