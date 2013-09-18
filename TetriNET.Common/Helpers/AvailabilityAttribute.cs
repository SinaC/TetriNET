using System;

namespace TetriNET.Common.Helpers
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AvailabilityAttribute : Attribute
    {
        public bool Available { get; private set; }
        public char ShortName { get; private set; }
        public string LongName { get; private set; }

        public AvailabilityAttribute(bool available)
        {
            Available = available;
        }

        public AvailabilityAttribute(bool available, char shortName, string longName) : this(available)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
