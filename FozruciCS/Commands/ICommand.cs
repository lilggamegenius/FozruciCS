using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Logging;
using DSharpPlus;
using FozruciCS.Links;
using FozruciCS.Listeners;
using Newtonsoft.Json;

namespace FozruciCS.Commands{
	public interface ICommand{
		Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
		Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e);
	}

	public abstract class ISaveable<T>
		where T:ISaveable<T>{
		private const string PATH_FORMAT = "Data/{0}/{1}.json";
		private static readonly ILog Logger = LogManager.GetLogger<ISaveable<T>>();
		protected virtual string name=>GetType().Name;
		protected virtual string folder=>GetType().Name;
		public virtual async Task SaveData(T data){
			await Task.Run(()=>{
				FileInfo file = new FileInfo(string.Format(PATH_FORMAT, folder, name));
				Directory.CreateDirectory(file.DirectoryName);
				using(JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(file.Create()))){
					Program.Serializer.Serialize(textWriter, data ?? this);
					Logger.Debug($"Saving data to file {file.FullName}");
				}
			});
		}
		public virtual async Task<T> LoadData(){
			FileInfo file = new FileInfo(string.Format(PATH_FORMAT, folder, name));
			if(!file.Exists){
				await SaveData(null);
				return (T)this;
			}

			return await Task.Run(()=>{
				using(StreamReader sr = new StreamReader(file.OpenRead()))
				using(JsonTextReader reader = new JsonTextReader(sr)){
					Logger.Debug($"Loading data from file {file.FullName}");
					return Program.Serializer.Deserialize<T>(reader);
				}
			});
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
