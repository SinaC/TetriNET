// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ServiceModel.Description;

namespace ServiceModelEx
{
   public abstract class AddressesContainer<T> : IEnumerable<EndpointAddress>,IEnumerable<KeyValuePair<EndpointAddress,Collection<Uri>>>,IDisposable where T : class
   {
      protected readonly Dictionary<EndpointAddress,Collection<Uri>> Dictionary;

      protected string Namespace
      {
         get
         {
            ContractDescription description = ContractDescription.GetContract(typeof(T));
            return description.Namespace;
         }
      }
      public EndpointAddress[] Addresses
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            return Dictionary.Keys.ToArray();
         }
      }

      public AddressesContainer() 
      {
         Dictionary = new Dictionary<EndpointAddress,Collection<Uri>>();
      }

      public AddressesContainer(AddressesContainer<T> container) 
      {
         lock(container)
         {
            foreach(EndpointAddress address in container)
            {
               EndpointAddress addressCopy = new EndpointAddress(address.Uri.AbsoluteUri);
               Dictionary[addressCopy] = CloneCollection(container[address]);
            }
         }
      }
      public abstract void Open();
  
      public abstract void Close();

      [MethodImpl(MethodImplOptions.Synchronized)]
      public static Dictionary<EndpointAddress,Collection<Uri>> FindUnion(AddressesContainer<T> container1,AddressesContainer<T> container2)
      {
         lock(container1)
         lock(container2)
         {
            Dictionary<EndpointAddress,Collection<Uri>> union = new Dictionary<EndpointAddress,Collection<Uri>>();

            foreach(EndpointAddress address in container1)
            {
               union[new EndpointAddress(address.Uri.AbsoluteUri)] = CloneCollection(container1.Dictionary[address]);
            }

            string[] addresses = union.Keys.Select((address)=>address.Uri.AbsoluteUri).ToArray();

            foreach(EndpointAddress address in container2)
            {
               if(addresses.Contains(address.Uri.AbsoluteUri) == false)
               {
                  union[new EndpointAddress(address.Uri.AbsoluteUri)] = CloneCollection(container2.Dictionary[address]);
               }
            }
            return union;
         }
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public void Remove(EndpointAddress address)
      {
         EndpointAddress addressToRemove = null;

         foreach(EndpointAddress endpointAddress in Dictionary.Keys)
         {
            string address1 = endpointAddress.Uri.AbsoluteUri;
            string address2 = address.Uri.AbsoluteUri;

            if(address1.EndsWith("/") == false)
            {
               address1 += "/";
            }            
            if(address2.EndsWith("/") == false)
            {
               address2 += "/";
            }
            if(address1 == address2)
            {
               addressToRemove = endpointAddress;
            }
         }
         if(addressToRemove != null)
         {
            Dictionary.Remove(addressToRemove);
         }
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      public EndpointAddress[] Find(Uri scope)
      {
         if(scope == null)
         {
            return Addresses;
         }
         List<EndpointAddress> list = new List<EndpointAddress>();
         foreach(EndpointAddress address in Dictionary.Keys)
         {
            string[] scopes = Dictionary[address].Select((uri) => uri.AbsoluteUri).ToArray();
            if(scopes.Contains(scope.AbsoluteUri))
            {
               list.Add(address);
            }
         }
         return list.ToArray();
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public EndpointAddress[] FindComplement(Uri scopeToExlude)
      {
         List<EndpointAddress> list = new List<EndpointAddress>();
         foreach(EndpointAddress address in Dictionary.Keys)
         {
            string[] scopes = Dictionary[address].Select((uri) => uri.AbsoluteUri).ToArray();
            if(scopes.Contains(scopeToExlude.AbsoluteUri) == false)
            {
               list.Add(address);
            }
         }
         return list.ToArray();
      }
      [MethodImpl(MethodImplOptions.Synchronized)]
      public IEnumerator<EndpointAddress> GetEnumerator()
      {
         foreach(EndpointAddress address in Dictionary.Keys)
         {
            yield return address;
         }
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
         IEnumerable<Uri> enumerator = this as IEnumerable<Uri>;
         return enumerator.GetEnumerator();
      }
      public EndpointAddress this[int index]
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            return Dictionary.Keys.ToArray()[index];
         }
      }
      public Collection<Uri> this[EndpointAddress address]
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            return CloneCollection(Dictionary[address]);
         }
      }
      public int Count
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get
         {
            return Dictionary.Count;
         }
      }
      public void Dispose()
      {
         Close();
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      IEnumerator<KeyValuePair<EndpointAddress,Collection<Uri>>> IEnumerable<KeyValuePair<EndpointAddress,Collection<Uri>>>.GetEnumerator()
      {
         foreach(KeyValuePair<EndpointAddress,Collection<Uri>> pair in Dictionary)
         {
            yield return pair;
         }      
      } 
      static Collection<U> CloneCollection<U>(Collection<U> source)
      {
         lock(source)
         {
            Collection<U> clone = new Collection<U>();
            foreach(U item in source)
            {
               clone.Add(item);
            }
            return clone;
         }
      }
   }
}


