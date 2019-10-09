using System;
using System.Collections.Generic;
using FozruciCS.Links;

namespace FozruciCS.Listeners{
	public interface IListener{
		void ExitHandler(object sender, EventArgs args);
		void LogMessage(LinkedChannel channel, LinkedMessage message);
		List<LinkedMessage> GetMessages(LinkedChannel channel);
	}
}
