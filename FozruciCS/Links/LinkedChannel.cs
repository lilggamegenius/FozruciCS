using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatSharp;
using DSharpPlus.Entities;

namespace FozruciCS.Links{
	public abstract class LinkedChannel{
		public abstract bool isIrc{get;}
		public abstract bool isDiscord{get;}
		public abstract string name{get;}

		public abstract Task respond(String message, LinkedUser user = null);
	}

	public class LinkedIrcChannel : LinkedChannel{
		public IrcChannel channel{get;}

		public override bool isIrc=>true;
		public override bool isDiscord=>false;

		public override string name{get;}

		public LinkedIrcChannel(IrcChannel channel){
			this.channel = channel;
			name = channel.Name;
		}

		public override async Task respond(string message, LinkedUser user = null){
			await Task.Run(()=>{
				if(Program.Config.AutoSplitMessage){
					string[] messages = message.Split('\r', '\n');
					foreach(string messageToSend in messages){
						if(messageToSend.Length == 0){
							continue;
						}
						channel.SendMessage($"{(user != null ? $"{user.nick}: " : "")}{messageToSend}");
					}
				} else{
					channel.SendMessage($"{(user != null ? $"{user.nick}: " : "")}{message.Replace('\r', ' ').Replace('\n', ' ')}");
				}
			});
		}

		public static implicit operator IrcChannel(LinkedIrcChannel channel){return channel.channel;}
		public static implicit operator LinkedIrcChannel(IrcChannel channel){return new LinkedIrcChannel(channel);}
	}

	public class LinkedDiscordChannel : LinkedChannel{
		public DiscordChannel channel{get;}

		public override bool isIrc=>false;
		public override bool isDiscord=>true;

		public override string name{get;}

		public LinkedDiscordChannel(DiscordChannel channel){
			this.channel = channel;
			name = channel.Name;
		}

		public override async Task respond(string message, LinkedUser user = null){
			await channel.SendMessageAsync($"{(user != null ? $"{((LinkedDiscordUser)user).DiscordMember.Mention}: " : "")}{message}");
		}

		public static implicit operator DiscordChannel(LinkedDiscordChannel channel){return channel.channel;}
		public static implicit operator LinkedDiscordChannel(DiscordChannel channel){return new LinkedDiscordChannel(channel);}
	}
}
