using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace POC.SignalR
{
    public class SignalRServer
    {
        public void Start()
        {
            string url = "http://localhost:9090";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }

    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ////app.MapSignalR();
            //app.New();
            var config = new HubConfiguration
            {
                EnableCrossDomain = true,
                //EnableJSONP = true
                EnableDetailedErrors = true
            };
            app.MapHubs(config);
        }
    }

    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }
    }
}
