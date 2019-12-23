namespace FozruciCS.Commands{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FozruciCS.Links;
	using FozruciCS.Listeners;
	using NLog;
	using NMaier.GetOptNet;

	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.BotOwner)]
	public class ClearMessages : ICommand{
		internal const string Usage = "Usage: Send <channel>";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static ClearMessages(){
			Program.RegisterCommand(nameof(Send), new Send());
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			var opts = new ClearMessagesOptions();
			try{
				opts.Parse(args);
				if(opts.regex &&
				   opts.mask){
					throw new InvalidValueException("Regex and Mask formatting cannot both be used");
				}
			}
			catch(GetOptException ex){
				await Help(listener, respondTo, args, e);
			}
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new ClearMessagesOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Send.Usage, UsageEpilog = Send.Epilogue)]
	public class ClearMessagesOptions : GetOpt{
		[Argument, ShortArgument('c')] public int count = 0;

		[Argument, ShortArgument('m')] public bool mask = false;
		[Parameters(Max = 1)] public List<string> Parameters = new List<string>();

		// Mutually Exclusive
		[Argument, ShortArgument('r')] public bool regex = false;
	}
}
