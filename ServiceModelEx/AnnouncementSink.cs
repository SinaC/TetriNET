// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Xml;
using System.Threading;

namespace ServiceModelEx
{
   public class AnnouncementSink<T> : AddressesContainer<T> where T : class
   {
      readonly ServiceHost m_Host;

      public event Action<string> OnHelloEvent = delegate{};
      public event Action<string> OnByeEvent = delegate{};
   
      public AnnouncementSink() 
      {
         AnnouncementService announcementService = new AnnouncementService();
         m_Host = new ServiceHost(announcementService);
         m_Host.Description.Behaviors.Find<ServiceBehaviorAttribute>().UseSynchronizationContext = false;

         m_Host.AddServiceEndpoint(new UdpAnnouncementEndpoint());

         announcementService.OfflineAnnouncementReceived += OnBye;
         announcementService.OnlineAnnouncementReceived  += OnHello;  
      }
      public override void Open()
      {
         m_Host.Open();
      }      
      public override void Close()
      {
         m_Host.Close();
      }
      void OnHello(object sender,AnnouncementEventArgs args)
      {      
         foreach(XmlQualifiedName contract in args.EndpointDiscoveryMetadata.ContractTypeNames)
         {
            if(contract.Name == typeof(T).Name && contract.Namespace == Namespace)
            {
               PublishNotificationEvent(OnHelloEvent,args.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri);
               Dictionary[args.EndpointDiscoveryMetadata.Address] = args.EndpointDiscoveryMetadata.Scopes;
            }
         }
      }
      void OnBye(object sender,AnnouncementEventArgs args)
      {
         foreach(XmlQualifiedName contract in args.EndpointDiscoveryMetadata.ContractTypeNames)
         {
            if(contract.Name == typeof(T).Name && contract.Namespace == Namespace)
            {
               PublishNotificationEvent(OnByeEvent,args.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri);

               Debug.Assert(Dictionary.ContainsKey(args.EndpointDiscoveryMetadata.Address));

               Dictionary.Remove(args.EndpointDiscoveryMetadata.Address);
            }
         }
      }
      void PublishNotificationEvent(Action<string> notification,string address)
      {
         Delegate[] subscribers = notification.GetInvocationList();
         WaitCallback fire = (state)=>
                             {
                                Action<string> subscriber = state as Action<string>;
                                subscriber(address);
                             };
         foreach(Delegate subscriber in subscribers)
         {
            
            ThreadPool.QueueUserWorkItem(fire,subscriber);        
         }
      }
   }
}