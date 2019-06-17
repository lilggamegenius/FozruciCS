using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using FozruciCS.Utils;
using Newtonsoft.Json;

namespace FozruciCS.Commands{
	// ReSharper disable once UnusedMember.Global
	// Loaded via reflection
	[PermissionLevel(Modes.None)]
	public class Markov : ICommand{
		internal const string Usage = "Usage: Markov";
		internal const string Epilogue = "This command has no options";

		private const int MaxGenLength = 150;
		private static readonly ILog Logger = LogManager.GetLogger<Markov>();
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
			timer = new Timer(async e=>await SaveData(), null, periodTimeSpan, periodTimeSpan);
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond(await GenerateMessage());}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){await respondTo.respond("");}

		public async Task onMessage(IListener listener, IRespondable respondTo, LinkedMessage e){await AddMessage(e.message);}

		public async Task LoadData(){
			if(MarkovData != null){ return; }

			MarkovData tempMarkov = new MarkovData();
			MarkovData = await tempMarkov.LoadData();
			if(MarkovData == null){ MarkovData = tempMarkov; }
		}

		public async Task SaveData(){
			if(MarkovData == null){ return; }

			foreach(List<string> chains in MarkovData.Values){ chains.Sort(); }

			await MarkovData.SaveData();
		}
		public async Task<string> GenerateMessage(int wordLimit = 50)=>
			await Task.Run(()=>{
				if(MarkovData[MarkovData.start].Count == 0){ return "Markov Chain database is empty, Try saying some things!"; }

				StringBuilder builder = new StringBuilder();
				string lastWord = MarkovData.start;
				for(int i = 0; (builder.Length < MaxGenLength) || (i > wordLimit); i++){
					List<string> words = MarkovData[lastWord];
					if(words.Count == 0){
						i--;
						continue; // This should never happen
					}

					lastWord = words[LilGUtil.RandInt(0, words.Count - 1)];
					if(lastWord == MarkovData.end){ break; }

					builder.Append(lastWord).Append(" ");
				}

				return builder.ToString();
			});

		public async Task AddMessage(string message)=>
			await Task.Run(()=>{
				string[] words = message.ToLower().Split(' ');
				string word;
				string nextWord;
				if(words.Length > 1){
					for(int i = 0; i < words.Length; i++){
						word = words[i];
						nextWord = i == (words.Length - 1) ? MarkovData.end : words[i + 1];
						if(i == 0){
							MarkovData[MarkovData.start].Add(word);
							MarkovData[word].Add(nextWord);
							continue;
						}

						MarkovData[word].Add(nextWord);
						if(nextWord == MarkovData.end){
							MarkovData[word].Add(nextWord); // Add extra weight to try and make sentences end more often
						}
					}

					return;
				}

				word = words[0];
				MarkovData[MarkovData.start].Add(word);
				MarkovData[word].Add(MarkovData.end);
				MarkovData[word].Add(MarkovData.end); // Add extra weight to try and make sentences end more often
			});
	}

	public class MarkovData : ISaveable<MarkovData>{
		public const string start = "\u0002";
		public const string end = "\u0003";
		[JsonProperty] private Dictionary<string, List<string>> MarkovChain = new Dictionary<string, List<string>>{
			{start, new List<string>()}
		};
		public List<string> this[string key]{
			get{
				if(MarkovChain.TryGetValue(key, out List<string> ret)){ return ret; }

				// else
				List<string> empty = new List<string>();
				this[key] = empty;
				return empty;
			}
			set=>MarkovChain[key] = value;
		}

		[JsonIgnore] public Dictionary<string, List<string>>.KeyCollection Keys=>MarkovChain.Keys;
		[JsonIgnore] public Dictionary<string, List<string>>.ValueCollection Values=>MarkovChain.Values;
	}
}
