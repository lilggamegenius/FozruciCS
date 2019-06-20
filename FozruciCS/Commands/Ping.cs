using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel]
	public class Ping : ICommand{
		internal const string Usage = "Usage: ping";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<Ping>();
		static Ping(){Program.RegisterCommand(nameof(Ping), new Ping());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("Pong");}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond(">_>");}
	}
}
