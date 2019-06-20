using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using FozruciCS.Links;
using FozruciCS.Listeners;
using FozruciCS.Utils;

namespace FozruciCS.Commands{
	[PermissionLevel]
	public class Magic8Ball : ICommand{
		internal const string Usage = "Usage: 8Ball";
		internal const string Epilogue = "This command has no options";
		private static readonly ILog Logger = LogManager.GetLogger<Magic8Ball>();
		static Magic8Ball(){
			Magic8Ball magic8Ball = new Magic8Ball();
			Program.RegisterCommand("8Ball", magic8Ball);
		}

		public async Task HandleCommand(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){
			int choice = LilGUtil.RandInt(1, 20);
			string response = "";
			switch(choice){
				case 1:
					response = "It is certain";
					break;
				case 2:
					response = "It is decidedly so";
					break;
				case 3:
					response = "Without a doubt";
					break;
				case 4:
					response = "Yes - definitely";
					break;
				case 5:
					response = "You may rely on it";
					break;
				case 6:
					response = "As I see it, yes";
					break;
				case 7:
					response = "Most likely";
					break;
				case 8:
					response = "Outlook good";
					break;
				case 9:
					response = "Signs point to yes";
					break;
				case 10:
					response = "Yes";
					break;
				case 11:
					response = "Reply hazy, try again";
					break;
				case 12:
					response = "Ask again later";
					break;
				case 13:
					response = "Better not tell you now";
					break;
				case 14:
					response = "Cannot predict now";
					break;
				case 15:
					response = "Concentrate and ask again";
					break;
				case 16:
					response = "Don't count on it";
					break;
				case 17:
					response = "My reply is no";
					break;
				case 18:
					response = "My sources say no";
					break;
				case 19:
					response = "Outlook not so good";
					break;
				case 20:
					response = "Very doubtful";
					break;
			}

			await respondTo.respond(response, e.author);
		}

		#pragma warning disable 1998
		public async Task Help(IListener listener, IRespondable respondTo, IList<string> args, LinkedMessage e){}
		#pragma warning restore 1998
	}
}
