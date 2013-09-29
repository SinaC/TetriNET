// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Linq;

namespace ServiceModelEx
{
   public class ServiceHost<T> : ServiceHost
   {
      /// <summary>
      /// Can only call before openning the host
      /// </summary>
      public void EnableMetadataExchange(bool enableHttpGet = true)
      {
         if(State == CommunicationState.Opened)
         {
            throw new InvalidOperationException("Host is already opened");
         }

         ServiceMetadataBehavior metadataBehavior;
         metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();

         if(metadataBehavior == null)
         {
            metadataBehavior = new ServiceMetadataBehavior();
            Description.Behaviors.Add(metadataBehavior);
                                                            
            if(BaseAddresses.Any((uri)=>uri.Scheme == "http"))
            {
               metadataBehavior.HttpGetEnabled = enableHttpGet;
            }
                                              
            if(BaseAddresses.Any((uri)=>uri.Scheme == "https"))
            {
               metadataBehavior.HttpsGetEnabled = enableHttpGet;
            }
         }
         AddAllMexEndPoints();
      }
      public void AddAllMexEndPoints()
      {
         Debug.Assert(HasMexEndpoint == false);

         foreach(Uri baseAddress in BaseAddresses)
         {
            Binding binding = null;
            switch(baseAddress.Scheme)
            {
               case "net.tcp":
               {
                  binding = MetadataExchangeBindings.CreateMexTcpBinding();
                  break;
               }
               case "net.pipe":
               {
                  binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
                  break;
               }
               case "http":
               {
                  binding = MetadataExchangeBindings.CreateMexHttpBinding();
                  break;
               }
               case "https":
               {
                  binding = MetadataExchangeBindings.CreateMexHttpsBinding();
                  break;
               }
            }
            if(binding != null)
            {
               AddServiceEndpoint(typeof(IMetadataExchange),binding,"MEX");
            }         
         }
      }
      
      public bool HasMexEndpoint
      {
         get
         {
            return Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IMetadataExchange));
         }
      }

      public ServiceHost() : base(typeof(T))
      {}
      public ServiceHost(params string[] baseAddresses) : base(typeof(T),baseAddresses.Select((address)=>new Uri(address)).ToArray())
      {}
      public ServiceHost(params Uri[] baseAddresses) : base(typeof(T),baseAddresses)
      {}
      public ServiceHost(T singleton,params string[] baseAddresses) : base(singleton,baseAddresses.Select((address)=>new Uri(address)).ToArray())
      {}
      public ServiceHost(T singleton) : base(singleton)
      {}
      public ServiceHost(T singleton,params Uri[] baseAddresses) : base(singleton,baseAddresses)
      {}
      public virtual T Singleton
      {
         get
         {
            if(SingletonInstance == null)
            {
               return default(T);
            }
            Debug.Assert(SingletonInstance is T);
            return (T)SingletonInstance;
         }
      }
   }
}





