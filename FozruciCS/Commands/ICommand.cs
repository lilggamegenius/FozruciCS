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
		public virtual async Task SaveData(T data = null){
			await Task.Run(()=>{
				FileInfo file = new FileInfo(string.Format(PATH_FORMAT + ".temp", folder, name));
				FileInfo oldFile = new FileInfo(string.Format(PATH_FORMAT, folder, name));
				if(file.DirectoryName == null){
					return; // Why would this ever be null?
				}

				Directory.CreateDirectory(file.DirectoryName);
				using(JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(file.Create()))){
					Program.Serializer.Serialize(textWriter, data ?? this); // Save to temporary directory
					Logger.Debug($"Saving data to file {file.FullName}");
				}

				oldFile.Delete();
				file.MoveTo(oldFile.FullName); // Move to correct location
				Logger.Debug("Replaced save file with temp file");
			});
		}
		public virtual async Task<T> LoadData(){
			FileInfo file = new FileInfo(string.Format(PATH_FORMAT, folder, name));
			if(file.Exists){
				return await Task.Run(()=>{
					using(StreamReader sr = new StreamReader(file.OpenRead()))
					using(JsonTextReader reader = new JsonTextReader(sr)){
						Logger.Debug($"Loading data from file {file.FullName}");
						T temp = Program.Serializer.Deserialize<T>(reader);
						return temp;
					}
				});
			}

			await SaveData();
			return (T)this;
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
		public PermissionLevel(Modes minLevel = Modes.None, Permissions requiredPermission = Permissions.None){
			this.minLevel = minLevel;
			this.requiredPermission = Permissions.None;
		}
		public Modes minLevel{get;}
		public Permissions requiredPermission{get;}
	}

	public enum Modes{ None, Voice, Halfop, Op, SuperOp, Owner, BotOwner }
}
