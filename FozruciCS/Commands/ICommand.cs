using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using FozruciCS.Links;
using FozruciCS.Listeners;
using Newtonsoft.Json;

namespace FozruciCS.Commands{
	public interface ICommand{
		Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
		Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
	}

	public abstract class ISaveable{
		public virtual string path=>GetType().Name;
		public virtual async Task SaveData(){
			using(JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(new FileInfo($"Data/{path}.json").Open(FileMode.Create)))){
				Program.Serializer.Serialize(textWriter, this);
			}
		}
		public virtual async Task<ISaveable> LoadData(){
			using(StreamReader sr = new StreamReader(new FileInfo($"Data/{path}.json").OpenRead()))
			using(JsonTextReader reader = new JsonTextReader(sr)){ return Program.Serializer.Deserialize<ISaveable>(reader); }
		}
	}

	public class CommandList{
		public delegate Task OnMessageDelegate(IListener listener, IRespondable respondTo, LinkedMessage e);
		public CommandList()=>commands = new Dictionary<string, ICommand>();
		public Dictionary<string, ICommand> commands{get;}

		public ICommand this[string i]{
			get=>commands[i.ToLower()];
			set=>commands[i.ToLower()] = value;
		}
		public event OnMessageDelegate onMessage;

		public async Task message(IListener listener, IRespondable respondTo, LinkedMessage e){
			if(onMessage != null){ await onMessage(listener, respondTo, e); }
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
