using System;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class Mapper
    {
        public static char MapSpecialToChar(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? '?' : (attribute.ShortName == 0 ? '?' : attribute.ShortName);
        }

        public static string MapSpecialToString(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return (attribute == null || String.IsNullOrEmpty(attribute.LongName)) ? special.ToString() : attribute.LongName;
        }
    }
}
