using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using Jint;
using NMaier.GetOptNet;
using static FozruciCS.Utils.LilGUtil;

namespace FozruciCS.Commands{
	[PermissionLevel]
	public class JS : ICommand{
		internal const string Usage = "Usage: JS <code>";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<JS>();
		private readonly Engine OpEngine;

		private readonly Engine UserEngine;

		static JS(){Program.RegisterCommand(nameof(JS), new JS());}

		public JS(){
			UserEngine = new Engine();
			OpEngine = new Engine(cfg=>cfg.AllowClr());
			OpEngine.SetValue("Config", Program.Config);
			OpEngine.SetValue("CommandList", Program.CommandList);
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			JSOptions opts = new JSOptions();
			try{
				opts.Parse(args);
				string code;
				if(opts.Parameters.Count > 1){ code = ArgJoiner(opts.Parameters.ToArray()); } else{ code = opts.Parameters[0]; }

				bool owner = CheckIfOwner(e);
				Engine engine;
				if(owner){
					engine = OpEngine;
					engine.SetValue("listener", listener);
					engine.SetValue("respondTo", respondTo);
					engine.SetValue("args", args);
					engine.SetValue("e", e);
				} else{ engine = UserEngine; }

				Logger.Info($"Executing JS code as {(owner ? "Owner" : "Normal User")}: {code}");
				await respondTo.respond(engine.Execute(code).GetCompletionValue().ToString(), e.author);
			} catch(GetOptException){ await Help(listener, respondTo, args, e); }
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new JSOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = JS.Usage, UsageEpilog = JS.Epilogue)]
	public class JSOptions : GetOpt{
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();
	}
}