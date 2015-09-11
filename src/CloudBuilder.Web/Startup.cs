using Microsoft.Owin;
using Owin;

[assembly: OwinStartup (typeof (CloudBuilder.Web.Startup))]

namespace CloudBuilder.Web
{
	public class Startup
	{
		public void Configuration (IAppBuilder app)
		{
			app.MapSignalR ();
		}
	}
}
