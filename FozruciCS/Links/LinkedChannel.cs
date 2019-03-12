using System.Threading;
using System.Threading.Tasks;
using ChatSharp;
using DSharpPlus.Entities;

namespace FozruciCS.Links{
	public abstract class LinkedChannel : IRespondable{
		public abstract bool isIrc{get;}
		public abstract bool isDiscord{get;}
		public abstract string name{get;}

		public abstract Task respond(string message, LinkedUser user = null);
	}

	public class LinkedIrcChannel : LinkedChannel{
		public LinkedIrcChannel(IrcChannel channel){
			this.channel = channel;
			name = channel.Name;
		}
		public IrcChannel channel{get;}

		public override bool isIrc=>true;
		public override bool isDiscord=>false;

		public override string name{get;}

		public override async Task respond(string message, LinkedUser user = null){
			await Task.Run(()=>{
				if(Program.Config.AutoSplitMessage){
					string[] messages = message.Split('\r', '\n');
					foreach(string messageToSend in messages){
						if(messageToSend.Length == 0){ continue; }

						channel.SendMessage($"{(user != null ? $"{user.nick}: " : "")}{messageToSend}");
						Thread.Sleep(50);
					}
				} else{ channel.SendMessage($"{(user != null ? $"{user.nick}: " : "")}{message.Replace('\r', ' ').Replace('\n', ' ')}"); }
			});
		}

		public static implicit operator IrcChannel(LinkedIrcChannel channel)=>channel.channel;
		public static implicit operator LinkedIrcChannel(IrcChannel channel)=>new LinkedIrcChannel(channel);
	}

	public class LinkedDiscordChannel : LinkedChannel{
		public LinkedDiscordChannel(DiscordChannel channel){
			this.channel = channel;
			name = channel.Name;
		}
		public DiscordChannel channel{get;}

		public override bool isIrc=>false;
		public override bool isDiscord=>true;

		public override string name{get;}

		public override async Task respond(string message, LinkedUser user = null){
			await channel.SendMessageAsync($"{(user != null ? $"{((LinkedDiscordUser)user).DiscordMember.Mention}: " : "")}{message}");
		}

		public static implicit operator DiscordChannel(LinkedDiscordChannel channel)=>channel.channel;
		public static implicit operator LinkedDiscordChannel(DiscordChannel channel)=>new LinkedDiscordChannel(channel);
	}
}
