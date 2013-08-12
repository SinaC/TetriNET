using System;
using System.Web;
using System.Net;
using IPFiltering.Configuration;

namespace IPFiltering
{
    /// <summary>
    /// Http module that filters requests based on their ip address
    /// </summary>
    public class IPFilterModule : IHttpModule
    {

        private static readonly ThreadSafeSingleton<IPFilter> _filter = new ThreadSafeSingleton<IPFilter>(Create);

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <value>The filter.</value>
        protected static IPFilter Filter
        {
            get
            {
                return _filter.Value;
            }
        }
        
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
        }

        /// <summary>
        /// Handles the BeginRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app != null)
            {
                HttpContext context = app.Context;
                if (context != null && context.Request.UserHostAddress != null)
                {
                    IPAddress address = IPAddress.Parse(context.Request.UserHostAddress);
                    IPFilterTypes result = Filter.CheckAddress(address);
                    if (result == IPFilterTypes.Deny)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.Output.Write("<html><body>Access Denied<body></html>");
                        context.Response.End();
                    }
                }
            }
        }

        private static IPFilter Create()
        {
            return FilterFactory.Create(IPFilterConfiguration.Default.Filters[IPFilterConfiguration.Default.HttpModuleConfig.FilterName]);

        }
    }
}
