namespace FozruciCS.Commands{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FozruciCS.Links;
	using FozruciCS.Listeners;
	using NLog;

	[PermissionLevel]
	public class Egg : ICommand{
		internal const string Usage = "Usage: egg";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static Egg(){
			Program.RegisterCommand(nameof(Egg), new Egg());
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond("🥚");
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond("Would you like an 🥚 in this trying time");
		}
	}
}
