using System.Collections.Generic;
using ChatSharp;
using DSharpPlus;
using FozruciCS.Listeners;
using Newtonsoft.Json;

namespace FozruciCS.Config{
	public class Configuration{
		private const string DefaultName = "FozruciCS";
		public bool AutoSplitMessage = false;

		public string[] CommandPrefixes = {};
		public ulong DiscordBotOwnerID;

		public Dictionary<string, DiscordChannelConfiguration> discordChannelOptions;

		[JsonIgnore] public DiscordListener DiscordListener;
		[JsonIgnore] public DiscordClient DiscordSocketClient;
		public string DiscordToken = "<Missing discord token in config>";
		public bool FloodProtection = true;
		public int FloodProtectionDelay = 1000;
		public string[] GithubCreds;
		public string GithubGistOAuthToken;
		public string Modes = "+B";
		public string Nickname = DefaultName;
		public string RealName = "";

		public Dictionary<string, ServerConfiguration> servers;
		public string UserName = DefaultName;

		public bool Equals(Configuration other)=>string.Equals(Nickname, other.Nickname)                         &&
												 string.Equals(UserName, other.UserName)                         &&
												 string.Equals(RealName, other.RealName)                         &&
												 string.Equals(Modes, other.Modes)                               &&
												 (AutoSplitMessage     == other.AutoSplitMessage)                &&
												 (FloodProtection      == other.FloodProtection)                 &&
												 (FloodProtectionDelay == other.FloodProtectionDelay)            &&
												 string.Equals(DiscordToken, other.DiscordToken)                 &&
												 (DiscordBotOwnerID == other.DiscordBotOwnerID)                  &&
												 string.Equals(GithubGistOAuthToken, other.GithubGistOAuthToken) &&
												 Equals(GithubCreds, other.GithubCreds)                          &&
												 Equals(discordChannelOptions, other.discordChannelOptions)      &&
												 Equals(servers, other.servers);
		public override bool Equals(object obj){
			if(ReferenceEquals(null, obj)){ return false; }

			return obj is Configuration other && Equals(other);
		}
		public override int GetHashCode(){
			unchecked{
				int hashCode = Nickname                 != null ? Nickname.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (UserName != null ? UserName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (RealName != null ? RealName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Modes    != null ? Modes.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ AutoSplitMessage.GetHashCode();
				hashCode = (hashCode * 397) ^ FloodProtection.GetHashCode();
				hashCode = (hashCode * 397) ^ FloodProtectionDelay;
				hashCode = (hashCode * 397) ^ (DiscordToken != null ? DiscordToken.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ DiscordBotOwnerID.GetHashCode();
				hashCode = (hashCode * 397) ^ (GithubGistOAuthToken  != null ? GithubGistOAuthToken.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (GithubCreds           != null ? GithubCreds.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (discordChannelOptions != null ? discordChannelOptions.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (servers               != null ? servers.GetHashCode() : 0);
				return hashCode;
			}
		}
		public static bool operator==(Configuration left, Configuration right)=>(left != null) && left.Equals(right);
		public static bool operator!=(Configuration left, Configuration right)=>(left != null) && !left.Equals(right);

		public class ServerConfiguration{
			[JsonProperty("commandprefixes")] private string[] _commandPrefixes;
			[JsonProperty("nickname")] private string _nickname = DefaultName; // for overrides
			[JsonProperty("realname")] private string _realName;
			[JsonProperty("username")] private string _userName = DefaultName;
			public string botOwnerHostmask;
			public Dictionary<string, IrcChannelConfiguration> channelOptions;
			public bool IgnoreInvalidSSL;
			[JsonIgnore] public IrcClient IrcClient;

			[JsonIgnore] public IrcListener IrcListener;
			[JsonIgnore] public IrcUser IrcSelf;

			public string nickservAccountName;
			public string nickservPassword;
			public ushort port = 6667; // = 6667 or 6697 for ssl

			public string server;
			public string serverPassword;
			public bool SSL = false;

			public string NickName=>_nickname                 ?? Program.Config.Nickname;
			public string UserName=>_userName                 ?? Program.Config.UserName;
			public string RealName=>_realName                 ?? Program.Config.RealName;
			public string[] CommandPrefixes=>_commandPrefixes ?? Program.Config.CommandPrefixes;
		}
	}
}
