using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace TetriNET.WPF_WCF_Client.MVVM
{
    public static class Mediator
    {
        private class Subscription
        {
            private WeakReference RecipientWeakReference { get; }

            public object Recipient => RecipientWeakReference?.Target;

            public object Token { get; }
            public MethodInfo Method { get; }
            public SynchronizationContext Context { get; }

            public bool IsAlive
            {
                get
                {
                    if (RecipientWeakReference == null)
                        return false;
                    return RecipientWeakReference.IsAlive;
                }
            }

            public Subscription(object recipient, MethodInfo method, object token, SynchronizationContext context)
            {
                RecipientWeakReference = new WeakReference(recipient);
                Method = method;
                Token = token;
                Context = context;
            }
        }

        private static readonly Dictionary<Type, List<Subscription>> Recipients = new Dictionary<Type, List<Subscription>>();

        #region Register
        public static void Register<T>(object recipient, Action<T> action)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            Register(recipient, null, action);
        }

        public static void Register<T>(object recipient, object token, Action<T> action)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            lock (Recipients)
            {
                Type messageType = typeof(T);

                List<Subscription> subscriptions;
                if (!Recipients.ContainsKey(messageType))
                {
                    subscriptions = new List<Subscription>();
                    Recipients.Add(messageType, subscriptions);
                }
                else
                    subscriptions = Recipients[messageType];

                lock (subscriptions)
                {
                    subscriptions.Add(new Subscription(recipient, action.Method, token, SynchronizationContext.Current));
                }
            }
            Cleanup();
        }
        #endregion

        #region Unregister
        public static void Unregister(object recipient)
        {
            lock (Recipients)
            {
                foreach (KeyValuePair<Type, List<Subscription>> kv in Recipients)
                    UnregisterFromList(kv.Value, x => x.Recipient == recipient);
            }
        }

        public static void Unregister<T>(object recipient)
        {
            lock (Recipients)
            {
                Type messageType = typeof(T);
                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient);
            }
        }

        public static void Unregister<T>(object recipient, Action<T> action)
        {
            lock (Recipients)
            {
                Type messageType = typeof(T);
                MethodInfo method = action.Method;

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Method == method);
            }
        }

        public static void Unregister<T>(object recipient, object token)
        {
            lock (Recipients)
            {
                Type messageType = typeof(T);

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Token == token);
            }
        }

        public static void Unregister<T>(object recipient, object token, Action<T> action)
        {
            lock (Recipients)
            {
                Type messageType = typeof(T);
                MethodInfo method = action.Method;

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Method == method && x.Token == token);
            }
        }

        //
        private static void UnregisterFromList(List<Subscription> list, Func<Subscription, bool> filter)
        {
            lock (list)
            {
                List<Subscription> toRemove = list.Where(filter).ToList();
                foreach (Subscription item in toRemove)
                    list.Remove(item);
            }
            Cleanup();
        }
        #endregion

        #region Send
        public static void Send<T>(T message)
        {
            Send(message, null);
        }

        public static void Send<T>(T message, object token)
        {
            List<Subscription> clone = null;
            lock (Recipients)
            {
                Type messageType = typeof(T);

                if (Recipients.ContainsKey(messageType))
                {
                    // Clone to avoid problem if register/unregistering in "receive message" method
                    lock (Recipients[messageType])
                    {
                        clone = Recipients[messageType].Where(x => (x.Token == null && token == null)
                                                                   ||
                                                                   (x.Token != null && x.Token.Equals(token))
                            ).ToList();
                    }
                }
            }
            if (clone != null)
                SendToList(clone, message);
        }

        private static void SendToList<T>(IEnumerable<Subscription> list, T message)
        {
            // Send message to matching recipients
            List<Exception> exceptions = new List<Exception>();
            foreach (Subscription item in list)
            {
                try
                {
                    if (item.IsAlive)
                    {
                        //http://stackoverflow.com/questions/4843010/net-how-do-i-invoke-a-delegate-on-a-specific-thread-isynchronizeinvoke-disp
                        // Execute on thread which performed Register if possibe
                        Subscription subscription = item; // avoid closure problem
                        if (subscription.Context != null)
                        {
                            subscription.Context.Post(
                                _ => subscription.Method.Invoke(subscription.Recipient, new object[] { message }), null);
                        }
                        else // no context specified while registering, create a delegate and BeginInvoke
                        {
                            Func<object, object[], object> delegateToMethod = subscription.Method.Invoke;
                            delegateToMethod.BeginInvoke(subscription.Recipient, new object[] { message }, null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (exceptions.Any())
                throw new AggregateException("Send problem", exceptions);
            Cleanup();
        }
        #endregion

        #region Cleanup
        private static void Cleanup()
        {
            // Clean dead recipients
            lock (Recipients)
            {
                foreach (KeyValuePair<Type, List<Subscription>> kv in Recipients)
                {
                    List<Subscription> list = kv.Value;
                    List<Subscription> toRemove = list.Where(x => !x.IsAlive || x.Recipient == null).ToList();
                    foreach (Subscription item in toRemove)
                        list.Remove(item);
                }
            }
        }
        #endregion
    }
}
