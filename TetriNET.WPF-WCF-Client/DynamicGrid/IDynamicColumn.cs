using System;

namespace TetriNET.WPF_WCF_Client.DynamicGrid
{
    public interface IDynamicColumn
    {
        string Name { get; }
        string DisplayName { get; }
        Type Type { get; }
        bool IsReadOnly { get; }
    }
}
