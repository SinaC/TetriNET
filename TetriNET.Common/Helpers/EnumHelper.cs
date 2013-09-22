using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;

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

        public static IEnumerable<Pieces> GetPieces(Func<bool, bool> filter = null)
        {
            return Enum.GetValues(typeof(Pieces)).Cast<Pieces>()
                .Select(x => new
                {
                    enumValue = x,
                    attribute = GetAttribute<PieceAttribute>(x)
                })
                .Where(x => x.attribute != null && (filter == null || filter(x.attribute.Available)))
                .Select(x => x.enumValue);
        }

        public static IEnumerable<Specials> GetSpecials(Func<bool, bool> filter = null )
        {
            return Enum.GetValues(typeof(Specials)).Cast<Specials>()
                .Select(x => new
                {
                    enumValue = x,
                    attribute = GetAttribute<SpecialAttribute>(x)
                })
                .Where(x => x.attribute != null && (filter == null || filter(x.attribute.Available)))
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
