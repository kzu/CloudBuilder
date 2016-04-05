using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CloudBuilder.Web
{
	[HubName ("build")]
	public class BuildHub : Hub
	{
		public Task Listen (string buildId)
		{
			return Groups.Add (Context.ConnectionId, buildId);
		}

		public void Message (string buildId, string message)
		{
			Clients.Group (buildId).Message (message);
		}

		/// <summary>
		/// Signals the attached logger that the Shutdown message was received on the server.
		/// </summary>
		public void Shutdown (string buildId)
		{
			Clients.Group (buildId).Shutdown ();
		}
	}
}