using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FozruciCS.Links;

namespace FozruciCS.Listeners{
	public interface IListener{
		void ExitHandler(object sender, EventArgs args);
		void LogMessage(LinkedChannel channel, LinkedMessage message);
		Task<List<LinkedMessage>> GetMessages(LinkedChannel channel);
	}
}
