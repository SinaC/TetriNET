using System;

namespace TetriNET.Common.Attributes
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PieceAttribute : Attribute
    {
        public bool Available { get; private set; }
        public string Name { get; private set; }

        public PieceAttribute(bool available)
        {
            Available = available;
        }

        public PieceAttribute(bool available, string name)
            : this(available)
        {
            Name = name;
        }
    }
}
