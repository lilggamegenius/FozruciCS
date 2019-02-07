using System.Collections.Generic;
using System.Threading.Tasks;
using ChatSharp;
using ChatSharp.Events;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FozruciCS.Links;
using FozruciCS.Listeners;

namespace FozruciCS.Commands{
	public interface ICommand{
		Task HandleCommand(IListener listener, LinkedChannel channel, IList<string> args, LinkedMessage e);
		Task Help(IListener listener, LinkedChannel channel, IList<string> args, LinkedMessage e);


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
}
