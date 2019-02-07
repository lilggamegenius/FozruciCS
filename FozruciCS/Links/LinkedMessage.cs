using System.Collections.Generic;
using ChatSharp;
using ChatSharp.Events;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace FozruciCS.Links{
	public abstract class LinkedMessage{
		public abstract LinkedUser author{get;}
		public abstract LinkedChannel channel{get;}
		public abstract LinkedServer server{get;}
		public abstract string message{get;}

	}

	public class LinkedIrcMessage : LinkedMessage{
		public override LinkedUser author{get;}
		public override LinkedChannel channel{get;}
		public override LinkedServer server{get;}
		public override string message{get;}
		public PrivateMessageEventArgs PrivateMessageEventArgs{get;}

		public LinkedIrcMessage(LinkedUser author, LinkedChannel channel, LinkedServer server, string message, PrivateMessageEventArgs privateMessageEventArgs){
			this.author = author;
			this.channel = channel;
			this.server = server;
			this.message = message;
			PrivateMessageEventArgs = privateMessageEventArgs;
		}

		public static implicit operator LinkedIrcMessage(PrivateMessageEventArgs privateMessageEventArgs){
			PrivateMessage privateMessage = privateMessageEventArgs.PrivateMessage;
			return new LinkedIrcMessage((LinkedIrcUser)privateMessage.User, (LinkedIrcChannel)privateMessage.Channel, (LinkedIrcServer)privateMessage.User.ServerInfo, privateMessage.Message, privateMessageEventArgs);
		}

		public static implicit operator PrivateMessageEventArgs(LinkedIrcMessage message){return message.PrivateMessageEventArgs;}
	}


	public class LinkedDiscordMessage : LinkedMessage{
		public override LinkedUser author{get;}
		public override LinkedChannel channel{get;}
		public override LinkedServer server{get;}
		public override string message{get;}
		public IReadOnlyList<DiscordReaction> Reactions{get;}
		public IReadOnlyList<DiscordRole> MentionedRoles{get;}
		public IReadOnlyList<DiscordUser> MentionedUsers{get;}
		public IReadOnlyList<DiscordChannel> MentionedChannels{get;}
		public MessageType? MessageType{get;}
		public LinkedDiscordMessage(LinkedDiscordUser author, LinkedDiscordChannel channel, LinkedDiscordServer server, string message, IReadOnlyList<DiscordReaction> reactions, IReadOnlyList<DiscordRole> mentionedRoles, IReadOnlyList<DiscordUser> mentionedUsers, IReadOnlyList<DiscordChannel> mentionedChannels, MessageType? messageType){
			this.author = author;
			this.channel = channel;
			this.server = server;
			this.message = message;
			Reactions = reactions;
			MentionedRoles = mentionedRoles;
			MentionedUsers = mentionedUsers;
			MentionedChannels = mentionedChannels;
			MessageType = messageType;
		}

		public static implicit operator LinkedDiscordMessage(MessageCreateEventArgs message){
			return new LinkedDiscordMessage(new LinkedDiscordUser(message.Message.Author, message.Guild),
														 message.Message.Channel,
														 message.Message.Channel.Guild,
														 message.Message.Content,
														 message.Message.Reactions,
														 message.Message.MentionedRoles,
														 message.Message.MentionedUsers,
														 message.Message.MentionedChannels,
														 message.Message.MessageType);
		}
	}
}
