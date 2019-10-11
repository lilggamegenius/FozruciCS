using System.Threading.Tasks;
using ChatSharp;
using DSharpPlus.Entities;

namespace FozruciCS.Links{
	public abstract class LinkedUser : IRespondable{
		public abstract bool isIrc{get;}
		public abstract bool isDiscord{get;}
		public abstract string nick{get;}
		public abstract string username{get;}
		public abstract string id{get;}
		public string hostmask=>$"{nick}!{username}@{id}";

		public abstract Task respond(string message, LinkedUser user = null);
		public override string ToString()=>$"<{hostmask}>";
	}

	public class LinkedIrcUser : LinkedUser{
		private LinkedIrcUser(IrcUser user)=>IrcUser = user;
		public IrcUser IrcUser{get;}

		public override bool isIrc=>true;
		public override bool isDiscord=>false;

		public override string nick=>IrcUser.Nick;
		public override string username=>IrcUser.User;
		public override string id=>IrcUser.Hostname;

		public override async Task respond(string message, LinkedUser user = null){
			await Task.Run(()=>{
				if(Program.Config.AutoSplitMessage){
					string[] messages = message.Split('\r', '\n');
					foreach(string messageToSend in messages){
						if(messageToSend.Length == 0){ continue; }

						IrcUser.SendMessage(messageToSend);
					}
				} else{ IrcUser.SendMessage(message.Replace('\r', ' ').Replace('\n', ' ')); }
			});
		}

		public static implicit operator IrcUser(LinkedIrcUser user)=>user.IrcUser;
		public static implicit operator LinkedIrcUser(IrcUser user)=>new LinkedIrcUser(user);
	}

	public class LinkedDiscordUser : LinkedUser{
		public LinkedDiscordUser(DiscordMember member){
			DiscordMember = member;
			DiscordUser = member;
		}

		public LinkedDiscordUser(DiscordUser user, DiscordGuild guild){
			DiscordUser = user;
			DiscordMember = guild.GetMemberAsync(user.Id).Result;
		}
		public DiscordMember DiscordMember{get;}
		public DiscordUser DiscordUser{get;}

		public override bool isIrc=>false;
		public override bool isDiscord=>true;

		public override string nick=>DiscordMember.DisplayName;
		public override string username=>DiscordMember.Username;
		public override string id=>DiscordMember.Id.ToString();

		public bool HasNick=>DiscordMember.Nickname != null;

		public override async Task respond(string message, LinkedUser user = null){await DiscordMember.SendMessageAsync(message);}

		public static implicit operator DiscordMember(LinkedDiscordUser user)=>user.DiscordMember;
		public static implicit operator DiscordUser(LinkedDiscordUser user)=>user.DiscordUser;
		public static implicit operator LinkedDiscordUser(DiscordMember user)=>new LinkedDiscordUser(user);
	}
}
