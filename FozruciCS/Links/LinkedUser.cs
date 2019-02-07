using ChatSharp;
using DSharpPlus.Entities;

namespace FozruciCS.Links{
	public abstract class LinkedUser{
		public abstract bool isIrc{get;}
		public abstract bool isDiscord{get;}
		public abstract string nick{get;}
		public abstract string username{get;}
		public abstract string id{get;}

		public abstract void respond(string message);
	}

	public class LinkedIrcUser : LinkedUser{
		public IrcUser IrcUser{get;}

		public override bool isIrc=>true;
		public override bool isDiscord=>false;

		public override string nick=>IrcUser.Nick;
		public override string username=>IrcUser.User;
		public override string id=>IrcUser.Hostname;

		private LinkedIrcUser(IrcUser user){IrcUser = user;}

		public override void respond(string message){
			IrcUser.SendMessage(message);
		}

		public static implicit operator IrcUser(LinkedIrcUser user){return user.IrcUser;}
		public static implicit operator LinkedIrcUser(IrcUser user){
			return new LinkedIrcUser(user);
		}

	}

	public class LinkedDiscordUser : LinkedUser{
		public DiscordMember DiscordMember{get;}
		public DiscordUser DiscordUser{get;}

		public override bool isIrc=>false;
		public override bool isDiscord=>true;

		public override string nick=>DiscordMember.DisplayName;
		public override string username=>DiscordMember.Username;
		public override string id=>DiscordMember.Id.ToString();

		public bool HasNick=>DiscordMember.Nickname != null;

		public LinkedDiscordUser(DiscordMember member){
			DiscordMember = member;
			DiscordUser = member;
		}

		public LinkedDiscordUser(DiscordUser user, DiscordGuild guild){
			DiscordUser = user;
			DiscordMember = guild.GetMemberAsync(user.Id).Result;
		}


		public override async void respond(string message){
			await DiscordMember.SendMessageAsync(message);
		}

		public static implicit operator DiscordMember(LinkedDiscordUser user){return user.DiscordMember;}
		public static implicit operator DiscordUser(LinkedDiscordUser user){return user.DiscordUser;}
		public static implicit operator LinkedDiscordUser(DiscordMember user){
			return new LinkedDiscordUser(user);
		}
	}
}
