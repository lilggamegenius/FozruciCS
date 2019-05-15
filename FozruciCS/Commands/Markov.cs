using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.None)]
	public class Markov : ICommand{
		internal const string Usage = "Usage: Markov";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<Markov>();
		static Markov(){
			Markov markov = new Markov();
			Program.RegisterCommand(nameof(Markov), markov);
			Program.CommandList.onMessage += markov.onMessage;
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("Pong");}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond(">_>");}

		public async Task onMessage(IListener listener, IRespondable respondTo, LinkedMessage e){}
	}
}
