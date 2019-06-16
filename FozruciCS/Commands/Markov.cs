using System;
using System.Collections.Generic;
using System.Threading;
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
		private static readonly TimeSpan startTimeSpan = TimeSpan.Zero;
		private static readonly TimeSpan periodTimeSpan = TimeSpan.FromMinutes(1);

		private MarkovData MarkovData;

		private Timer timer;
		static Markov(){
			Markov markov = new Markov();
			Program.RegisterCommand(nameof(Markov), markov);
			Program.CommandList.onMessage += markov.onMessage;
		}

		public Markov(){
			Task.Run(async ()=>await LoadData());
			timer = new Timer(async e=>await SaveData(), null, startTimeSpan, periodTimeSpan);
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("");}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("");}

		public async Task onMessage(IListener listener, IRespondable respondTo, LinkedMessage e){await AddMessage(e.message);}

		public async Task LoadData(){
			if(MarkovData != null){ return; }

			MarkovData = await new MarkovData().LoadData();
		}

		public async Task SaveData(){
			if(MarkovData == null){ return; }

			await MarkovData.SaveData(null);
		}

		public async Task AddMessage(string message){}
	}

	public class MarkovData : ISaveable<MarkovData>{
		private const string start = "\u0002";
		private const string end = "\u0003";
		private Dictionary<string, List<string>> MarkovChain = new Dictionary<string, List<string>>{
			{start, new List<string>()},
			{end, new List<string>()}
		};
	}
}
