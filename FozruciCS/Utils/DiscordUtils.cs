using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using NLog;

namespace FozruciCS.Utils{
	public static class DiscordUtils{
		public const char ZeroWidthSpace = '\u200b';
		public const string Bold = "**";
		public const char Italics = '_';
		public const string Underline = "__";

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string FormatBold(this string str)=>$"{Bold}{str}{Bold}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string FormatItalics(this string str)=>$"{Italics}{str}{Italics}";
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string FormatUnderline(this string str)=>$"{Underline}{str}{Underline}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetHostMask(this DiscordUser user){
			DiscordMember member = user as DiscordMember;
			if(member != null){ return $"{member.DisplayName}!{user.Username}@{user.Id}"; }

			return $"{user.Username}!{user.Username}@{user.Id}";
		}

		public static Permissions GetPermissions(this DiscordMember member){
			Permissions permissions = Permissions.None;
			foreach(DiscordRole role in member.Roles){ permissions |= role.Permissions; }

			return permissions;
		}

		public static bool CanInteract(this DiscordMember issuer, DiscordMember target){
			if(issuer == null){ throw new ArgumentNullException(nameof(issuer)); }

			if(target == null){ throw new ArgumentNullException(nameof(target)); }

			DiscordGuild guild = issuer.Guild;
			if(guild != target.Guild){ throw new ArgumentException("Provided members must both be Member objects of the same Guild!"); }

			if(guild.Owner == issuer){ return true; }

			if(guild.Owner == target){ return false; }

			DiscordRole issuerRole = issuer.Roles.FirstOrDefault();
			DiscordRole targetRole = target.Roles.FirstOrDefault();
			return (issuerRole == null) && ((targetRole == null) || CanInteract(issuerRole, targetRole));
		}

		public static bool CanInteract(this DiscordRole issuer, DiscordRole target){
			if(issuer == null){ throw new ArgumentNullException(nameof(issuer)); }

			if(target == null){ throw new ArgumentNullException(nameof(target)); }

			return target.Position < issuer.Position;
		}

		public static string ToCommaSeperatedList(this IEnumerable<DiscordRole> array){
			StringBuilder builder = new StringBuilder();
			foreach(DiscordRole item in array){
				if(builder.Length != 0){ builder.Append(", "); }

				builder.Append(item.Name);
			}

			return builder.ToString();
		}
	}
}
