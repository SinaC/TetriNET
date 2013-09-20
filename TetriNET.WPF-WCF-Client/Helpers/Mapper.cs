using System;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class Mapper
    {
        public static char MapSpecialToChar(Specials special)
        {
            AvailabilityAttribute attribute = EnumHelper.GetAttribute<AvailabilityAttribute>(special);
            return attribute == null ? '?' : (attribute.ShortName == 0 ? '?' : attribute.ShortName);
        }

        public static string MapSpecialToString(Specials special)
        {
            AvailabilityAttribute attribute = EnumHelper.GetAttribute<AvailabilityAttribute>(special);
            return (attribute == null || String.IsNullOrEmpty(attribute.LongName)) ? special.ToString() : attribute.LongName;
        }
    }
}
