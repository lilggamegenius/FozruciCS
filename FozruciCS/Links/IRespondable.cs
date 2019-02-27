using System.Threading.Tasks;

namespace FozruciCS.Links{
	public interface IRespondable{
		Task respond(string message, LinkedUser user = null);
	}
}
