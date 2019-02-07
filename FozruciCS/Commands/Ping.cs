using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NMaier.GetOptNet;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	public class Ping : ICommand{
		internal const string Usage = "Usage: <botname> ping <username>";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<Ping>();
		static Ping(){
			Program.RegisterCommand(nameof(Ping), new Ping());
		}

		public async Task HandleCommand(IListener listener, LinkedChannel channel, IList<string> args, LinkedMessage e){
			PingOptions opts = new PingOptions();
			try{
				opts.Parse(args);
				string find = opts.Parameters[0];
				await channel.respond("Pong");
			} catch(GetOptException){ await Help(listener, channel, args, e); }
		}

		public async Task Help(IListener listener, LinkedChannel channel, IList<string> args, LinkedMessage e){
			await channel.respond(new PingOptions().AssembleUsage(int.MaxValue), e.author);
		}
	}


	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Ping.Usage, UsageEpilog = Ping.Epilogue)]
	public class PingOptions : GetOpt{
		[Parameters(Min = 1)] public List<string> Parameters = new List<string>();
	}
}
