using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Logging;
using DSharpPlus.Entities;
using FozruciCS.Commands;

namespace FozruciCS.Utils{
	public static class IrcUtils{
		public const char CtcpChar = '\u0001';
		public const char ColorChar = '\u0003';
		public const char BoldChar = '\u0002';
		public const char ItalicsChar = '\u001D';
		public const char UnderlineChar = '\u001F';
		public const char ReverseChar = '\u0016';
		public const char BellChar = '\u0007';
		public const char SymbolForBellChar = '␇';
		public const char NewLineChar = '\n';
		public const char SymbolForNewLineChar = '␤';
		public const char CharageReturnChar = '\r';
		public const char SymbolForCharageReturnChar = '␍';

		private const string EscapePrefix = "@!";
		private static readonly ILog Logger = LogManager.GetLogger(typeof(IrcUtils));

		public static Dictionary<DiscordChannel, DropOutStack<DiscordUser>> LastUserToSpeak = new Dictionary<DiscordChannel, DropOutStack<DiscordUser>>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToColor(this string str, byte color)=>color >= 16 ? $"{ColorChar}{str}" : $"{ColorChar}{color:00}{str}{ColorChar}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToBold(this string str)=>$"{BoldChar}{str}{BoldChar}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToItalics(this string str)=>$"{ItalicsChar}{str}{ItalicsChar}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToUnderline(this string str)=>$"{UnderlineChar}{str}{UnderlineChar}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static string ToCtcp(this string str)=>$"{CtcpChar}{str}{CtcpChar}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsCtcp(this string str)=>str.StartsWith(CtcpChar.ToString()) && str.EndsWith(CtcpChar.ToString());

		public static bool Contains(this string str, char ch){
			if(str == null){ return false; }

			foreach(char c in str){
				if(c == ch){ return true; }
			}

			return false;
		}

		public static Modes GetUserLevel(string levels){
			if(levels.Contains('q')){ return Modes.Owner; }

			if(levels.Contains('a')){ return Modes.SuperOp; }

			if(levels.Contains('o')){ return Modes.Op; }

			if(levels.Contains('h')){ return Modes.Halfop; }

			if(levels.Contains('v')){ return Modes.Voice; }

			return Modes.None;
		}

		public static char? GetUserLevelChar(Modes mode){
			switch(mode){
				case Modes.Voice:
					return 'v';
				case Modes.Halfop:
					return 'h';
				case Modes.Op:
					return 'o';
				case Modes.SuperOp:
					return 'a';
				case Modes.Owner:
					return 'q';
			}

			return null;
		}

		public static bool MatchHostMask(this string hostmask, string pattern){
			string nick = hostmask.Substring(0, hostmask.IndexOf("!", StringComparison.Ordinal));
			string userName = hostmask.Substring(hostmask.IndexOf("!", StringComparison.Ordinal) + 1, hostmask.IndexOf("@", StringComparison.Ordinal));
			string hostname = hostmask.Substring(hostmask.IndexOf("@", StringComparison.Ordinal) + 1);
			string patternNick = pattern.Substring(0, pattern.IndexOf("!", StringComparison.Ordinal));
			string patternUserName = pattern.Substring(pattern.IndexOf("!", StringComparison.Ordinal) + 1, pattern.IndexOf("@", StringComparison.Ordinal));
			string patternHostname = pattern.Substring(pattern.IndexOf("@", StringComparison.Ordinal) + 1);
			return hostname.WildCardMatch(patternHostname) && userName.WildCardMatch(patternUserName) && nick.WildCardMatch(patternNick);
		}
	}
}
