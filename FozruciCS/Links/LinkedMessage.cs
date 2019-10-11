using System.Collections.Generic;
using System.Text;
using ChatSharp;
using ChatSharp.Events;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FozruciCS.Utils;

namespace FozruciCS.Links{
	public abstract class LinkedMessage{
		public abstract LinkedUser author{get;}
		public abstract LinkedChannel channel{get;}
		public abstract LinkedServer server{get;}
		public abstract string message{get;}
		public abstract override string ToString();
	}

	public class LinkedIrcMessage : LinkedMessage{
		public LinkedIrcMessage(LinkedUser author, LinkedChannel channel, LinkedServer server, string message, PrivateMessageEventArgs privateMessageEventArgs){
			this.author = author;
			this.channel = channel;
			this.server = server;
			this.message = message;
			PrivateMessageEventArgs = privateMessageEventArgs;
		}
		public override LinkedUser author{get;}
		public override LinkedChannel channel{get;}
		public override LinkedServer server{get;}
		public override string message{get;}
		public PrivateMessageEventArgs PrivateMessageEventArgs{get;}

		public IrcClient Client=>PrivateMessageEventArgs.PrivateMessage.User.Client;
		public override string ToString()=>$"{author} {message}";

		public static implicit operator LinkedIrcMessage(PrivateMessageEventArgs privateMessageEventArgs){
			PrivateMessage privateMessage = privateMessageEventArgs.PrivateMessage;
			return new LinkedIrcMessage((LinkedIrcUser)privateMessage.User,
										privateMessage.Channel != null ? (LinkedIrcChannel)privateMessage.Channel : null,
										(LinkedIrcServer)privateMessage.User.ServerInfo,
										privateMessage.Message,
										privateMessageEventArgs);
		}

		public static implicit operator PrivateMessageEventArgs(LinkedIrcMessage message)=>message.PrivateMessageEventArgs;
	}

	public class LinkedDiscordMessage : LinkedMessage{
		public DiscordMessage discordMessage;
		public LinkedDiscordMessage(LinkedDiscordUser author,
									LinkedDiscordChannel channel,
									LinkedDiscordServer server,
									DiscordMessage discordMessage){
			this.author = author;
			this.channel = channel;
			this.server = server;
			this.discordMessage = discordMessage;
		}
		public override LinkedUser author{get;}
		public override LinkedChannel channel{get;}
		public override LinkedServer server{get;}
		public IReadOnlyList<DiscordReaction> Reactions=>discordMessage.Reactions;
		public IReadOnlyList<DiscordRole> MentionedRoles=>discordMessage.MentionedRoles;
		public IReadOnlyList<DiscordUser> MentionedUsers=>discordMessage.MentionedUsers;
		public IReadOnlyList<DiscordChannel> MentionedChannels=>discordMessage.MentionedChannels;
		public MessageType? MessageType=>discordMessage.MessageType;
		public override string message{
			get{
				// ReSharper disable once LocalVariableHidesMember
				string message = discordMessage.Content;
				IReadOnlyList<DiscordAttachment> attachments = discordMessage.Attachments;
				if(attachments.Count == 0){ return message; }

				StringBuilder builder = new StringBuilder(message);
				foreach(DiscordAttachment attachment in attachments){ builder.Append(attachment.Url).Append(" "); }

				return builder.ToString();
			}
		}
		public override string ToString()=>$"{author} {message.SanitizeForIRC().StripEmojis()}";

		public static implicit operator LinkedDiscordMessage(MessageCreateEventArgs message)=>
			new LinkedDiscordMessage(new LinkedDiscordUser(message.Message.Author, message.Guild),
									 message.Message.Channel,
									 message.Guild,
									 message.Message);

		public static implicit operator LinkedDiscordMessage(DiscordMessage message)=>
			new LinkedDiscordMessage(new LinkedDiscordUser(message.Author, message.Channel.Guild),
									 message.Channel,
									 message.Channel.Guild,
									 message);
	}
}
