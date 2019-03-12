using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NMaier.GetOptNet;
using Renci.SshNet;

namespace FozruciCS.Commands{
	[PermissionLevel(Modes.BotOwner)]
	public class Term : ICommand{
		internal const string Usage = "Usage: Term [options] <host>";
		private static readonly ILog Logger = LogManager.GetLogger<Term>();
		public string host, username, keypath;
		public char[] password;
		public SshClient SshClient;
		public string termChar = ">";

		static Term(){
			Term term = new Term();
			Program.RegisterCommand(nameof(Term), term);
			Program.CommandList.onMessage += term.onMessage;
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			if(args.Count < 1){
				await respondTo.respond("Missing arguments");
				return;
			}

			TerminalOptions opts = new TerminalOptions();
			try{
				opts.Parse(args);
				//string command;
				//if(opts.Parameters.Count > 1){ command = LilGUtil.ArgJoiner(opts.Parameters.ToArray()); } else{ command = opts.Parameters[0]; }
				await respondTo.respond("", e.author);
			} catch(GetOptException ex){
				await respondTo.respond(ex.Message, e.author);
				Task task = Help(listener, respondTo, args, e);
				Logger.Warn(ex.ToString());
				await task;
			}
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new TerminalOptions().AssembleUsage(byte.MaxValue));
		}

		public async Task onMessage(IListener listener, IRespondable respondTo, LinkedMessage e){
			if(e.message.StartsWith(termChar)){
				if(SshClient == null){ await respondTo.respond("Need to connect to a client first"); }
			}
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Term.Usage)]
	public class TerminalOptions : GetOpt{
		[Argument(HelpText = "Private key path"), ShortArgument('k')]
		public string Keypath = null;
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();

		[Argument(HelpText = "Password to connect, or for private key")]
		public string Password = null;

		[Argument(HelpText = "Port to connect to"), ShortArgument('p')]
		public ushort Port = 22;
	}
}
