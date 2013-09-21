using System;

namespace TetriNET.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SpecialAttribute : Attribute
    {
        public bool Available { get; private set; }
        public char ShortName { get; private set; }
        public string LongName { get; private set; }

        public SpecialAttribute(bool available)
        {
            Available = available;
        }

        public SpecialAttribute(bool available, char shortName, string longName)
            : this(available)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
