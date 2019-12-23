namespace FozruciCS.Commands{
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading.Tasks;
	using FozruciCS.Links;
	using FozruciCS.Listeners;
	using FozruciCS.Utils;
	using Microsoft.CodeAnalysis.CSharp.Scripting;
	using Microsoft.CodeAnalysis.Scripting;
	using NLog;
	using NMaier.GetOptNet;

	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.BotOwner)]
	public class Repl : ICommand{
		internal const string Usage = "Usage: Repl <code>";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private readonly ReplGlobals Globals = new ReplGlobals();

		static Repl(){
			Program.RegisterCommand(nameof(Repl), new Repl());
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			var opts = new ReplOptions();
			try{
				opts.Parse(args);
				string code;
				if(opts.Parameters.Count > 1){
					code = LilGUtil.ArgJoiner(opts.Parameters.ToArray());
				}
				else{
					code = opts.Parameters[0];
				}

				Globals.e = e;
				Globals.args = args;
				Globals.listener = listener;
				Globals.respondTo = respondTo;
				Globals.server = e.server;
				Globals.channel = e.channel;
				Globals.author = e.author;
				Task<object> task = CSharpScriptEngine.Execute(code, Globals);
				await respondTo.respond((await task).ToString(), e.author);
			}
			catch(GetOptException){
				await Help(listener, respondTo, args, e);
			}
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new ReplOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Repl.Usage, UsageEpilog = Repl.Epilogue)]
	public class ReplOptions : GetOpt{
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();
	}

	[SuppressMessage("ReSharper", "NotAccessedField.Global")]
	public class ReplGlobals{
		public IList<string> args;
		public LinkedUser author;
		public LinkedChannel channel;
		public LinkedMessage e;
		public IListener listener;
		public IRespondable respondTo;
		public LinkedServer server;
	}

	public static class CSharpScriptEngine{
		private static ScriptState<object> scriptState;
		public static async Task<object> Execute(string code, ReplGlobals replGlobals){
			return await Task.Run(()=>{
				scriptState = scriptState == null ? CSharpScript.RunAsync(code, globals: replGlobals).Result : scriptState.ContinueWithAsync(code).Result;
				if((scriptState.ReturnValue != null) &&
				   !string.IsNullOrEmpty(scriptState.ReturnValue.ToString())){
					return scriptState.ReturnValue;
				}

				return null;
			});
		}
	}
}
