using System;

namespace TetriNET.Common.Attributes
{
    [Flags]
    public enum Availabilities
    {
        NotAvailable = 0,
        Randomizable = 1,
        Displayable = 2,
        Available = Randomizable | Displayable,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PieceAttribute : Attribute
    {
        public Availabilities Availability { get; private set; }
        public string Name { get; private set; }

        public PieceAttribute(Availabilities availability)
        {
            Availability = availability;
        }

        public PieceAttribute(Availabilities availability, string name) : this(availability)
        {
            Name = name;
        }
    }
}
