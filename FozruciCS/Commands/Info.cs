using System.Collections.Generic;
using System.Threading.Tasks;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NLog;
using NMaier.GetOptNet;

namespace FozruciCS.Commands{
	[PermissionLevel]
	public class Info : ICommand{
		internal const string Usage = "Usage: help <Command>";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static Info(){
			Info info = new Info();
			Program.RegisterCommand(nameof(Info), info);
			Program.RegisterCommand("Help", info);
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			InfoOptions opts = new InfoOptions();
			try{
				opts.Parse(args);
				if(Program.CommandList.ContainsCommand(opts.Parameters[0])){ await Program.CommandList[opts.Parameters[0]].Help(listener, respondTo, args, e); } else{
					await respondTo.respond("That command doesn't exist.");
				}
			} catch(GetOptException){ await Help(listener, respondTo, args, e); }
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new InfoOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Info.Usage, UsageEpilog = Info.Epilogue)]
	public class InfoOptions : GetOpt{
		[Parameters(Exact = 1)] public List<string> Parameters = new List<string>();
	}
}
