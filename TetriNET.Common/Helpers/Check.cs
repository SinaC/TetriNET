using System;
using System.Reflection;

namespace TetriNET.Common.Helpers
{
    public static class Check
    {
        // Check if every events of instance are handled
        public static bool CheckEvents<T>(T instance)
        {
            Type t = instance.GetType();
            foreach (EventInfo e in t.GetEvents())
            {
                if (e.DeclaringType == null)
                    return false;
                FieldInfo fi = e.DeclaringType.GetField(e.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (fi == null)
                    return false;
                object value = fi.GetValue(instance);
                if (value == null)
                    return false;
            }
            return true;
        }
    }
}
