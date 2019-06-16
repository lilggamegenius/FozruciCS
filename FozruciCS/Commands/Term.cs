using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using NMaier.GetOptNet;
using Renci.SshNet;
using SshConfigParser;

namespace FozruciCS.Commands{
	[PermissionLevel(Modes.BotOwner)]
	public class Term : ICommand{
		internal const string Usage = "Usage: Term [options] <host>";
		private static readonly ILog Logger = LogManager.GetLogger<Term>();
		public string host, username, keypath, password;
		public SshClient SshClient;
		public string termChar = ">";

		static Term(){
			Term term = new Term();
			Program.RegisterCommand(nameof(Term), term);
			Program.CommandList.onMessage += term.onMessage;
		}
		public static string homePath=>(Environment.OSVersion.Platform == PlatformID.Unix) ||
									   (Environment.OSVersion.Platform == PlatformID.MacOSX)
										   ? Environment.GetEnvironmentVariable("HOME")
										   : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			if(args.Count < 1){
				await respondTo.respond("Missing arguments");
				return;
			}

			TerminalOptions opts = new TerminalOptions();
			try{
				opts.Parse(args);
				if(opts.Parameters.Count != 0){
					SshClient = CreateClient(opts);
					SshClient.ErrorOccurred += (sender, eventArgs)=>Logger.Error($"SSH Error: \n{eventArgs.Exception}");
					SshClient.Connect();
					await respondTo.respond("Connected", e.author);
				}
			} catch(GetOptException ex){
				await respondTo.respond(ex.Message, e.author);
				Task task = Help(listener, respondTo, args, e);
				Logger.Warn(ex.ToString());
				await task;
			}
		}

		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			await respondTo.respond(new TerminalOptions().AssembleUsage(byte.MaxValue));
		}

		public SshClient CreateClient(TerminalOptions opts){
			SshClient?.Dispose();
			host = opts.Parameters[0];
			SshConfig config = SshConfig.ParseFile(opts.ConfigFile);
			SshHost hostConfig = config.Compute(host);
			keypath = opts.Keypath ?? hostConfig.IdentityFile;
			if(keypath.Contains("~")){ keypath = keypath.Replace("~", homePath); }

			host = hostConfig.HostName;
			username = opts.User ?? hostConfig.User ?? Environment.UserName;
			password = opts.Password;
			ushort port = opts.Port;
			if(opts.Port == 0){ port = hostConfig["Port"] is string portStr ? ushort.Parse(portStr) : (ushort)22; }

			bool PasswordAuthUsed = false;
			if(keypath == null){
				if(password != null){ PasswordAuthUsed = true; } else{ PasswordAuthUsed = hostConfig["PasswordAuthentication"] as string == "Yes"; }
			}

			if(PasswordAuthUsed){ return new SshClient(host, port, username, password); }

			PrivateKeyFile privateKeyFile = new PrivateKeyFile(keypath, password);
			return new SshClient(host, port, username, privateKeyFile);
		}

		public async Task onMessage(IListener listener, IRespondable respondTo, LinkedMessage e){
			if(!e.message.StartsWith(termChar)){ return; }

			bool? connected = SshClient?.IsConnected;
			if((connected == null) ||
			   (bool)!connected){
				await respondTo.respond("Need to connect to a client first");
				return;
			}

			using(SshCommand command = SshClient.RunCommand(e.message.Substring(1))){ await respondTo.respond(command.Result); }
		}
	}

	[GetOptOptions(OnUnknownArgument = UnknownArgumentsAction.Ignore, UsageIntro = Term.Usage)]
	public class TerminalOptions : GetOpt{
		private List<string> _parameters = new List<string>();

		[Argument(HelpText = "SSH Config file to use"), ShortArgument('c')]
		public string ConfigFile = $"{Term.homePath}/.ssh/config";
		[Argument(HelpText = "Private key path"), ShortArgument('k')]
		public string Keypath = null;

		[Argument(HelpText = "Password to connect, or for private key"), ArgumentAlias("pass")]
		public string Password = null;

		[Argument(HelpText = "Port to connect to"), ShortArgument('p')]
		public ushort Port = 0; // 22

		[Argument(HelpText = "User to connect as"), ShortArgument('u')]
		public string User;
		[Parameters(Min = 1)]
		public List<string> Parameters{
			get=>_parameters;
			set{
				_parameters = value;
				if((_parameters.Count > 0) &&
				   _parameters[0].Contains("@")){
					string[] temp = _parameters[0].Split('@');
					User = temp[0];
					_parameters = new List<string>{
						temp[1]
					};
				}
			}
		}
	}
}
