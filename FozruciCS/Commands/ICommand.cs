using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using FozruciCS.Links;
using FozruciCS.Listeners;

namespace FozruciCS.Commands{
	public interface ICommand{
		Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
		Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
	}

	public class Commands{
		public Commands()=>commands = new Dictionary<string, ICommand>();
		public Dictionary<string, ICommand> commands{get;}

		public ICommand this[string i]{
			get=>commands[i.ToLower()];
			set=>commands[i.ToLower()] = value;
		}

		public bool ContainsCommand(string command)=>commands.ContainsKey(command.ToLower());
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PermissionLevel : Attribute{
		public PermissionLevel(){
			minLevel = Modes.None;
			requiredPermission = Permissions.None;
		}

		public PermissionLevel(Modes minLevel){
			this.minLevel = minLevel;
			requiredPermission = Permissions.None;
		}
		public Modes minLevel{get;}
		public Permissions requiredPermission{get;}
	}

	public enum Modes{ None, Voice, Halfop, Op, SuperOp, Owner, BotOwner }
}
