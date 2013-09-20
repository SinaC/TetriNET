using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TetriNET.Common.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
        {
            Type enumT = typeof(T);
            if (!enumT.IsEnum)
                throw new InvalidCastException("GetValues must be used on enum");

            return Enum.GetValues(enumT).Cast<T>();
        }

        public static IEnumerable<T> GetAvailableValues<T>(Availabilities availabilities)
        {
            Type enumT = typeof(T);
            if (!enumT.IsEnum)
                throw new InvalidCastException("GetValues must be used on enum");
            return Enum.GetValues(enumT).Cast<T>()
                .Select(x => new
                {
                    enumValue = x,
                    attribute = GetAttribute<AvailabilityAttribute>(x)
                })
                .Where(x => x.attribute != null && x.attribute.Available == availabilities)
                .Select(x => x.enumValue);
        }

        public static T GetAttribute<T>(object enumValue) where T:Attribute
        {
            if (enumValue == null)
                return default(T);

            Type valueType = enumValue.GetType();
            if (!valueType.IsEnum)
                throw new InvalidCastException("GetAttribute must be used on enum");
            FieldInfo field = valueType.GetField(enumValue.ToString());
            return Attribute.GetCustomAttribute(field, typeof(T)) as T;
        }
    }
}
