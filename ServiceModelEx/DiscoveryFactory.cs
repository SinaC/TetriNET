// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.Net.Security;


namespace ServiceModelEx
{
   public static class DiscoveryFactory
   {
      public static ServiceHost<T> CreateDiscoverableHost<T>(bool supportIpc = true)
      {
         ServiceHost<T> host;
         if(supportIpc == true)
         {
            host = new ServiceHost<T>(DiscoveryHelper.AvailableIpcBaseAddress,DiscoveryHelper.AvailableTcpBaseAddress);
         }
         else
         {
             host = new ServiceHost<T>(DiscoveryHelper.AvailableTcpBaseAddress);
         }
         host.EnableDiscovery();

         return host;
      }
      public static T CreateChannel<T>(Uri scope = null)
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

         Binding binding = endpoints[0].Binding;
         EndpointAddress address = endpoints[0].Address;

         return ChannelFactory<T>.CreateChannel(binding,address);
      }         
      public static T[] CreateChannels<T>(bool inferBinding = true)
      {
         if(inferBinding)
         {
            return CreateInferedChannels<T>();
         }
         else
         {
            return CreateChannelsFromMex<T>();
         }
      }

      static T[] CreateChannelsFromMex<T>()  
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria();

         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count > 0);

         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata mexEndpoint in discovered.Endpoints)
         {
            ServiceEndpoint[] endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.Uri.AbsoluteUri,typeof(T));

            foreach(ServiceEndpoint endpoint in endpoints)
            {
               T proxy = ChannelFactory<T>.CreateChannel(endpoint.Binding,endpoint.Address);
               list.Add(proxy);
            }
         }
         Debug.Assert(list.Count > 0);
         return list.ToArray();
      }

      static T[] CreateInferedChannels<T>()
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         FindResponse discovered = discoveryClient.Find(criteria);
         discoveryClient.Close();

         Debug.Assert(discovered.Endpoints.Count > 0);
         List<T> list = new List<T>();

         foreach(EndpointDiscoveryMetadata endpoint in discovered.Endpoints)
         {
            Binding binding = InferBindingFromUri(endpoint.Address.Uri);           
            T proxy = ChannelFactory<T>.CreateChannel(binding,endpoint.Address);
            list.Add(proxy);
         }
         return list.ToArray();
      }
      
      static Binding InferBindingFromUri(Uri address)
      {
         switch(address.Scheme)
         {
            case "net.tcp":
            {
               NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.Transport,true);
               tcpBinding.TransactionFlow = true;
               return tcpBinding;
            }
            case "net.pipe":
            {
               NetNamedPipeBinding ipcBinding = new NetNamedPipeBinding();
               ipcBinding.TransactionFlow = true;
               return ipcBinding;
            }
            case "net.msmq":
            {
               NetMsmqBinding msmqBinding = new NetMsmqBinding();
               msmqBinding.Security.Transport.MsmqProtectionLevel = ProtectionLevel.EncryptAndSign;
               return msmqBinding;
            }
            default:
            {
               throw new InvalidOperationException("Can only create a channel over TCP/IPC/MSMQ bindings");
            }
         }   
      }
   }
}


