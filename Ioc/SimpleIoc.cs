using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ioc
{
    public class EasyIoc
    {
        private readonly Dictionary<Type, Type> _interfaceToImplementationMap = new Dictionary<Type, Type>();

        private static EasyIoc _default;

        public static EasyIoc Default
        {
            get
            {
                return _default ?? (_default = new EasyIoc());
            }
        }

        // TODO: clear

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            return _interfaceToImplementationMap.ContainsKey(interfaceType);
        }

        public void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only an interface can be registered");

            Type implementationType = typeof (TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot register an interface or an abstract class");

            if (_interfaceToImplementationMap.ContainsKey(interfaceType))
            {
                if (_interfaceToImplementationMap[interfaceType] != implementationType)
                    throw new InvalidOperationException(String.Format("There is already a class registered for interface {0}", interfaceType.FullName));
            }
            else
                _interfaceToImplementationMap.Add(interfaceType, implementationType);
            // TODO: create on-the-fly a create func and store it
        }

        public void Register<TInterface>(Func<TInterface> createFunc)
        {
            // TODO: store create func
        }

        // TODO: unregister

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only an interface can be resolved");

            if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

            Type implementationType = _interfaceToImplementationMap[interfaceType];
            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                throw new Exception(String.Format("Cannot Resolve: No public constructor found in {0}.", implementationType.Name));

            // TODO: get first parameterless ctor
            ConstructorInfo constructor = constructorInfos.First();
            ParameterInfo[] parameterInfos = constructor.GetParameters();

            // TODO: handle ctor with parameters

            if (parameterInfos.Length != 0)
                throw new Exception(String.Format("Cannot Resolve: No parameterless constructor found"));

            return (TInterface) constructor.Invoke(null);
        }
    }
}
