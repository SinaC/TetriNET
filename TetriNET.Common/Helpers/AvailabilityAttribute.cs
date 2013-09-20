using System;
using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Helpers
{
    public enum Availabilities
    {
        Available,
        NotAvailable,
        Mutation,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AvailabilityAttribute : Attribute
    {
        public Availabilities Available { get; private set; }
        public char ShortName { get; private set; }
        public string LongName { get; private set; }
        public Pieces Mutation { get; private set; }

        public AvailabilityAttribute(Availabilities available)
        {
            Available = available;
        }

        public AvailabilityAttribute(Availabilities available, char shortName, string longName)
            : this(available)
        {
            ShortName = shortName;
            LongName = longName;
        }

        public AvailabilityAttribute(Availabilities available, Pieces mutation)
            : this(available)
        {
            Mutation = mutation;
        }
    }
}
