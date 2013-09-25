// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServiceModelEx
{
   public static class DiscoveryHelper
   {
      public static void EnableDiscovery(this ServiceHost host,bool enableMEX = true)
      {
         if(host.Description.Endpoints.Count == 0)
         {
            host.AddDefaultEndpoints();
         }
         host.AddServiceEndpoint(new UdpDiscoveryEndpoint());

         ServiceDiscoveryBehavior discovery = new ServiceDiscoveryBehavior();
         discovery.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
         host.Description.Behaviors.Add(discovery);

         if(enableMEX == true)
         {         
            host.Description.Behaviors.Add(new ServiceMetadataBehavior());

            foreach(Uri baseAddress in host.BaseAddresses)
            {
               Binding binding = null;
               if(baseAddress.Scheme == "net.tcp")
               {
                  binding = MetadataExchangeBindings.CreateMexTcpBinding();
               }
               if(baseAddress.Scheme == "net.pipe")
               {
                  binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
               }
               Debug.Assert(binding != null);
               if(binding != null)
               {
                  host.AddServiceEndpoint(typeof(IMetadataExchange),binding,"MEX");
               }         
            }         
         }
      }
      public static Uri AvailableTcpBaseAddress
      {
         get
         {
            return new Uri("net.tcp://localhost:" + FindAvailablePort() + "/");
         }
      }

      public static Uri AvailableIpcBaseAddress
      {
         get
         {
            return new Uri("net.pipe://localhost/" + Guid.NewGuid() + "/");
         }
      }
      static int FindAvailablePort()
      {
         Mutex mutex = new Mutex(false,"ServiceModelEx.DiscoveryHelper.FindAvailablePort");
         try
         {
            mutex.WaitOne();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any,0);
            using(Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp))
            {
               socket.Bind(endPoint);
               IPEndPoint local = (IPEndPoint)socket.LocalEndPoint;
               return local.Port;
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public static EndpointAddress DiscoverAddress<T>(Uri scope = null)
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         criteria.MaxResults = 1;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }

         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count == 1);

         return discovered.Endpoints[0].Address;
      }
      public static EndpointAddress[] DiscoverAddresses<T>(Uri scope = null)
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         return discovered.Endpoints.Select((endpoint)=>endpoint.Address).ToArray();
      }

      public static Binding DiscoverBinding<T>(Uri scope = null)
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());

         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();
         criteria.MaxResults = 1;
         if(scope != null)
         {
            criteria.Scopes.Add(scope);
         }
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count == 1);

         Uri mexAddress = discovered.Endpoints[0].Address.Uri;

         ServiceEndpoint[] endpoints = MetadataHelper.GetEndpoints(mexAddress.AbsoluteUri,typeof(T));

         Debug.Assert(endpoints.Length == 1);

         return endpoints[0].Binding;
      }
   }
}


