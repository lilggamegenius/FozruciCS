using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Common.Logging;
using FozruciCS.Commands;
using FozruciCS.Config;
using FozruciCS.Listeners;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace FozruciCS{
	internal class Program{
		public const string Version = "Fozruci C# v0.1";
		public const string ErrorMsg = ". If you see this a lot, add a issue on the Issue tracker https://github.com/lilggamegenuis/FozruciCS/issues";
		private const string KvircFlags = "\u00034\u000F";
		private const int Attempts = 10;
		private const int ConnectDelay = 15 * 1000;
		internal static readonly Commands.Commands Commands;
		private static readonly ILog Logger = LogManager.GetLogger<Program>();
		private static readonly JsonSerializer Serializer = new JsonSerializer();
		private static FileInfo _configFile;
		private static FileInfo _permissionsFile;
		public static Configuration Config;
		public static Dictionary<string, Dictionary<string, Dictionary<string, PermissionLevel>>> Permissions =
			new Dictionary<string, Dictionary<string, Dictionary<string, PermissionLevel>>>();
		public static Timer saveTimer = new Timer{Interval = 5 * 1000, AutoReset = true, Enabled = true};

		static Program(){
			Commands = new Commands.Commands();
			IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
											   .SelectMany(s=>s.GetTypes())
											   .Where(p=>typeof(ICommand).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
			foreach(Type command in types){
				RuntimeHelpers.RunClassConstructor(command.TypeHandle);
				Logger.InfoFormat("Loaded command {0} as {1}", command.Name, command.FullName);
			}

			saveTimer.Elapsed += (sender, args)=>{
				using(JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(_permissionsFile.Open(FileMode.Create)))){ Serializer.Serialize(textWriter, Permissions); }
			};
		}
		public static long CurrentTimeMillis=>DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

		public static int Main(string[] args){
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			Logger.Debug($"Current directory is {Directory.GetCurrentDirectory()}");
			string configFilePath;
			string permissionsFilePath;
			#if DEBUG
			configFilePath = "Data/debugConfig.json";
			permissionsFilePath = "Data/debugPermissions.json";
			#else
				configFilePath = "Data/config.json";
				permissionsFilePath = "Data/permissions.json";
			#endif
			_configFile = new FileInfo(configFilePath);
			_permissionsFile = new FileInfo(permissionsFilePath);
			Logger.Info("Config Path = "      + _configFile);
			Logger.Info("Permissions Path = " + _permissionsFile);
			try{
				if(!_configFile.Exists){ throw new FileNotFoundException(configFilePath); }

				using(StreamReader sr = new StreamReader(_configFile.OpenRead()))
				using(JsonTextReader reader = new JsonTextReader(sr)){
					Config = Serializer.Deserialize<Configuration>(reader);
					Array.Sort(Config.CommandPrefixes, (x, y)=>y.Length.CompareTo(x.Length));
					new Thread(()=>{Config.DiscordListener = new DiscordListener();}).Start();
					foreach(string serverKey in Config.servers.Keys){
						new Thread(()=>{Config.servers[serverKey].IrcListener = new IrcListener(Config.servers[serverKey]);}).Start();
					}
				}

				if(!_permissionsFile.Exists){
					using(JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(_permissionsFile.Open(FileMode.Create)))){
						Serializer.Serialize(textWriter, Permissions);
					}
				} else{
					using(StreamReader sr = new StreamReader(_permissionsFile.OpenRead()))
					using(JsonTextReader reader = new JsonTextReader(sr)){
						Dictionary<string, Dictionary<string, Dictionary<string, PermissionLevel>>> tempPermissions =
							Serializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, PermissionLevel>>>>(reader);
						Permissions = tempPermissions ?? new Dictionary<string, Dictionary<string, Dictionary<string, PermissionLevel>>>();
					}
				}

				//Application.Run(new Form());
				bool isExit = false;
				while(!isExit){
					Console.Write("> ");
					string command = Console.ReadLine();
					isExit = handleCommand(command);
				}

				//client.Disconnect();
			} catch(Exception e){
				Logger.Error($"Error starting bot\n{e}");
				return 1;
			}

			return 0;
		}
		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e){
			Exception exception = e.ExceptionObject as Exception;
			Logger.Fatal($"Unhandled Exception caught: {exception?.Message}\n{exception?.StackTrace}", exception);
		}

		public static void RegisterCommand(string commandName, ICommand command){Commands[commandName.ToLower()] = command;}

		private static bool handleCommand(string command){
			if(command.Equals("exit")){ return true; }

			return false;
		}
	}
}
