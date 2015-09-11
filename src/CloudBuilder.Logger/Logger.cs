using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace CloudBuilder
{
	public class Logger : ConsoleLogger
	{
		const string defaultHubUrl = "http://cloudbuilder.azurewebsites.net/";

		HubConnection connection;
		IHubProxy proxy;
		string hubUrl;
		string buildId;

		public Logger ()
			: base (LoggerVerbosity.Detailed)
		{
			base.WriteHandler = new WriteHandler (OnWrite);
		}

		public override void Shutdown ()
		{
			if (connection != null)
				connection.Dispose ();
		}

		void OnWrite (string message)
		{
			if (proxy == null) {
				if (string.IsNullOrEmpty (Parameters))
					// throw new LoggerException ("No build identifier was specified as a logger parameter.");
					return;

				buildId = Parameters.Split (';')
					.Select (x => x.Split ('='))
					.Where (x => x.Length == 2 && x[0].Equals ("buildId", StringComparison.OrdinalIgnoreCase))
					.Select (x => x[1])
					.FirstOrDefault ();

				if (string.IsNullOrEmpty (buildId))
					throw new LoggerException ("No buildId parameter value was specified as a logger parameter. Use buildId=ID after the logger assembly.");

				hubUrl = Parameters.Split (';')
					.Select (x => x.Split ('='))
					.Where (x => x.Length == 2 && x[0].Equals ("hubUrl", StringComparison.OrdinalIgnoreCase))
					.Select (x => x[1])
					.FirstOrDefault () ?? defaultHubUrl;

				connection = new HubConnection (hubUrl);
				proxy = connection.CreateHubProxy ("build");
				try {
					connection.Start ().Wait (3000);
				} catch (Exception e) {
					Trace.WriteLine ("Failed to connect to " + hubUrl + ": " + e.ToString ());
					connection.Dispose ();
					connection = null;
				}
			}

			// If connection failed, the connection will be null.
			if (connection != null) {
				proxy.Invoke ("Message", buildId, message)
					.ContinueWith (t =>
						Trace.WriteLine ("Failed to publish message."),
						CancellationToken.None,
						TaskContinuationOptions.OnlyOnFaulted,
						TaskScheduler.Default);
			}
		}

		void OnAnyEvent (object sender, BuildEventArgs e)
		{
			proxy.Invoke ("Message", Parameters, e.Message)
				.ContinueWith (t =>
					Trace.WriteLine ("Failed to publish message."),
					CancellationToken.None,
					TaskContinuationOptions.OnlyOnFaulted,
					TaskScheduler.Default);
		}
	}
}
