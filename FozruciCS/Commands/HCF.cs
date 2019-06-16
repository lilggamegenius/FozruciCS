using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Config;
using FozruciCS.Links;
using FozruciCS.Listeners;
using FozruciCS.Utils;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.BotOwner)]
	public class HCF : ICommand{
		private static readonly ILog Logger = LogManager.GetLogger<HCF>();
		static HCF(){Program.RegisterCommand(nameof(HCF), new HCF());}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond("Fore!");
			LilGUtil.Pause(1);
			await respondTo.respond("I mean five!");
			LilGUtil.Pause(1);
			await respondTo.respond("I mean fire!");
			LilGUtil.Pause(1);
			if(e.server.isIrc){
				foreach(Configuration.ServerConfiguration serversValue in Program.Config.servers.Values){ serversValue.IrcClient.Quit("🔥🔥🔥"); }

				LilGUtil.Pause(5);
			}

			Environment.Exit(0);
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			LinkedChannel channel = e.channel;
			await channel.respond(">_>");
		}
	}
}