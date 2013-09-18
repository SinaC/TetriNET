using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class Mapper
    {
        public static char MapSpecialToChar(Specials special)
        {
            AvailabilityAttribute attribute = EnumHelper.GetAttribute<AvailabilityAttribute>(special);
            return attribute == null ? '?' : attribute.ShortName;
        }

        public static string MapSpecialToString(Specials special)
        {
            AvailabilityAttribute attribute = EnumHelper.GetAttribute<AvailabilityAttribute>(special);
            return attribute == null ? special.ToString() : attribute.LongName;
        }
    }
}
