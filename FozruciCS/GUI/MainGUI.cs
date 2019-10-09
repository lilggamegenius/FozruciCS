using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ChatSharp;
using DSharpPlus;
using DSharpPlus.Entities;
using FozruciCS.Config;
using FozruciCS.Links;
using NLog;
using NLog.Targets;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace FozruciCS.GUI{
	public class MainGUI{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private static readonly MemoryTarget Target = LogManager.Configuration.FindTargetByName<MemoryTarget>("stream");
		public ListView chanListView;
		public Window chanListWin;
		public IList channels = new List<string>();
		public bool followMessages = true;
		public List<LinkedChannel> LinkedChannels = new List<LinkedChannel>();

		public List<LinkedServer> LinkedServers = new List<LinkedServer>();
		public Window mainWin;

		public MenuBar menu;

		public IList messages = new List<string>();
		public ListView messagesListView;
		public TextField messagesTextField;
		public Window messagesWin;
		public ScrollView scrollView;
		public int selectedChannel;
		public int selectedServer;
		public ListView serverListView;
		public IList servers = new List<string>();

		private Timer Timer;

		public MainGUI(){
			Application.Init();
			Toplevel top = Application.Top;

			// Creates a menubar, the item "New" has a help menu.
			menu = new MenuBar(new[]{
				new MenuBarItem("_File",
								new[]{
									new MenuItem("_Quit",
												 "",
												 ()=>{top.Running = false;}),
									new MenuItem("_Save",
												 "",
												 ()=>{ /*Todo*/
												 })
								})
			});
			top.Add(menu);

			// Creates the top-level window to show
			mainWin = new Window(Program.Config.Nickname){
				X = 0,
				Y = 1, // Leave one row for the toplevel menu

				// By using Dim.Fill(), it will automatically resize without manual intervention
				Width = Dim.Fill(),
				Height = Dim.Fill()
			};
			top.Add(mainWin);
			chanListWin = new Window("Channel List"){
				X = 0,
				Y = 0,
				Width = Dim.Percent(25),
				Height = Dim.Fill()
			};
			serverListView = new ListView{
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Percent(50),
				AllowsMarking = true,
				CanFocus = true
			};
			chanListView = new ListView{
				X = 0,
				Y = Pos.Bottom(serverListView) + 1,
				Width = Dim.Fill(),
				Height = Dim.Fill(1),
				AllowsMarking = true,
				CanFocus = true
			};
			serverListView.SelectedChanged += ()=>{
				if(serverListView.SelectedItem == selectedServer){ return; }

				selectedServer = serverListView.SelectedItem;
				UpdateChannels();
				if(chanListView.Source.Count == 0){ return; }

				chanListView.SelectedItem = 0;
			};
			chanListView.SelectedChanged += ()=>{
				if(chanListView.SelectedItem == selectedChannel){ return; }

				selectedChannel = chanListView.SelectedItem;
				UpdateChannels();
			};
			chanListWin.Add(serverListView);
			chanListWin.Add(chanListView);
			mainWin.Add(chanListWin);
			messagesWin = new Window("Message Log"){
				X = Pos.Right(chanListWin),
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Fill()
			};
			mainWin.Add(messagesWin);
			messagesListView = new ListView{
				X = 0,
				Y = 0,
				Width = Dim.Fill(),
				Height = Dim.Fill(1),
				AllowsMarking = true,
				CanFocus = true
			};
			TimeSpan periodTimeSpan = TimeSpan.FromSeconds(1);
			//IList testList = new List<string>(){"Testing"};
			//messagesListView.SetSource(testList);

			//messagesListView.SetSource((IList)Target.Logs);
			Timer = new Timer(e=>{
								  UpdateServers();
								  //UpdateChannels();
								  UpdateLogs();
							  },
							  null,
							  periodTimeSpan,
							  periodTimeSpan);
			messagesWin.Add(messagesListView);
			messagesTextField = new TextField(""){
				X = 0,
				Y = Pos.Bottom(messagesListView),
				Width = Dim.Fill(),
				Height = 1
			};
			messagesWin.Add(messagesTextField);
			top.SetFocus(messagesTextField);
			Colors.Base.Focus = Attribute.Make(Color.Black, Color.BrightRed);
			Colors.Base.Normal = Attribute.Make(Color.Red, Color.Black);
		}

		public void UpdateLogs(bool fullUpdate = false){
			int selectedItem = messagesListView.SelectedItem;
			int topItem = messagesListView.TopItem;
			//var follow = selectedItem == messages.Count - 1;
			bool follow = true;
			if(selectedServer == 0){ // Info logs are always 0
				messages = (IList)Target.Logs;
			} else{
				LinkedServer server = LinkedServers[selectedServer];
				if(server is LinkedIrcServer){} else if(server is LinkedDiscordServer){}
			}

			int followTop = messages.Count - messagesListView.Frame.Height;
			messagesListView.SetSource(messages);
			if(follow || fullUpdate){
				messagesListView.SelectedItem = messages.Count - 1;
				messagesListView.TopItem = followTop;
			} else{
				messagesListView.SelectedItem = selectedItem;
				messagesListView.TopItem = topItem;
			}
		}

		public void UpdateServers(){
			if((Program.Config.DiscordSocketClient == null) ||
			   (Program.Config.servers.Count       == 0)){ return; }

			List<string> serverList = new List<string>{"Info Logs"};
			List<LinkedServer> linkedServers = new List<LinkedServer>{null};
			foreach(DiscordGuild guild in Program.Config.DiscordSocketClient.Guilds.Values){
				serverList.Add($"Discord - {guild.Name}");
				linkedServers.Add((LinkedDiscordServer)guild);
			}

			foreach(KeyValuePair<string, Configuration.ServerConfiguration> configuration in Program.Config.servers){
				ServerInfo serverInfo = configuration.Value.IrcClient.ServerInfo;
				serverList.Add($"IRC - {serverInfo.NetworkName ?? configuration.Key}");
				linkedServers.Add((LinkedIrcServer)serverInfo);
			}

			int selectedItem = serverListView.SelectedItem;
			servers = serverList;
			LinkedServers = linkedServers;
			serverListView.SetSource(servers);
			if(servers.Count < selectedItem){ serverListView.SelectedItem = selectedItem; }
		}

		public void UpdateChannels(){
			if((Program.Config.DiscordSocketClient == null) ||
			   (Program.Config.servers.Count       == 0)){ return; }

			//messages = new List<string>();
			channels = new List<string>();
			chanListView.SetSource(channels);
			LinkedChannels = new List<LinkedChannel>();
			if(selectedServer == 0){
				selectedChannel = 0;
				return;
			}

			LinkedServer linkedServer = LinkedServers[selectedServer];
			if(linkedServer.isDiscord){
				LinkedDiscordServer server = (LinkedDiscordServer)linkedServer;
				foreach(DiscordChannel channel in server.DiscordGuild.Channels){
					if(channel.Type != ChannelType.Text){ continue; }

					channels.Add(channel.Name);
					LinkedChannels.Add((LinkedDiscordChannel)channel);
				}
			}

			if(linkedServer.isIrc){
				LinkedIrcServer server = (LinkedIrcServer)linkedServer;
				foreach(Configuration.ServerConfiguration serverConfiguration in Program.Config.servers.Values){
					if(serverConfiguration.IrcClient.ServerInfo != server.IrcServer){ continue; }

					foreach(IrcChannel channel in serverConfiguration.IrcClient.Channels){
						channels.Add(channel.Name);
						LinkedChannels.Add((LinkedIrcChannel)channel);
					}
				}
			}
		}
	}
}
