using ChatSharp;
using DSharpPlus.Entities;

namespace FozruciCS.Links{
	public abstract class LinkedServer{
		public abstract bool isIrc{get;}
		public abstract bool isDiscord{get;}
		public abstract string name{get;}

	}

	public class LinkedIrcServer : LinkedServer{
		public ServerInfo IrcServer{get;}

		public override bool isIrc=>true;
		public override bool isDiscord=>false;

		public override string name{get;}

		public static implicit operator ServerInfo(LinkedIrcServer server){return server.IrcServer;}
		public static implicit operator LinkedIrcServer(ServerInfo server){return new LinkedIrcServer(server);}

		public LinkedIrcServer(ServerInfo serverInfo){
			IrcServer = serverInfo;
			name = serverInfo.NetworkName;
		}
	}

	public class LinkedDiscordServer : LinkedServer{
		public DiscordGuild DiscordGuild{get;}

		public override bool isIrc=>false;
		public override bool isDiscord=>true;

		public override string name{get;}


		public static implicit operator DiscordGuild(LinkedDiscordServer server){return server.DiscordGuild;}
		public static implicit operator LinkedDiscordServer(DiscordGuild server){return new LinkedDiscordServer(server);}

		public LinkedDiscordServer(DiscordGuild discordGuild){
			DiscordGuild = discordGuild;
			name = discordGuild.Name;
		}
	}
}
