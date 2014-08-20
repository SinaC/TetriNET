using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace TetriNET.WPF_WCF_Client.DynamicGrid
{
    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private readonly string _displayName;
        private readonly Type _dynamicType;
        private readonly bool _isReadOnly;

        public DynamicPropertyDescriptor(string name, string displayName, Type type, bool isReadOnly = false)
            : base(name, null)
        {
            _displayName = displayName;
            _dynamicType = type;
            _isReadOnly = isReadOnly;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return GetDynamicMember(component, Name);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            SetDynamicMember(component, Name, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return typeof(object); }
        }

        public override bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public override Type PropertyType
        {
            get { return _dynamicType; }
        }

        public override string DisplayName
        {
            get { return _displayName; }
        }

        private static void SetDynamicMember(object obj, string memberName, object value)
        {
            var binder = Binder.SetMember(
                CSharpBinderFlags.None,
                memberName,
                obj.GetType(),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
            callsite.Target(callsite, obj, value);
        }

        private static object GetDynamicMember(object obj, string memberName)
        {
            var binder = Binder.GetMember(
                CSharpBinderFlags.None,
                memberName,
                obj.GetType(),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }
    }
}
