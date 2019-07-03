using System.Collections.Generic;
using System.Threading.Tasks;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NLog;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel]
	public class Ping : ICommand{
		internal const string Usage = "Usage: ping";
		internal const string Epilogue = "This command has no options";
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		static Ping(){Program.RegisterCommand(nameof(Ping), new Ping());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("Pong");}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond(">_>");}
	}
}
