using System;

namespace TetriNET.WPF_WCF_Client.DynamicGrid
{
    public class DynamicColumn : IDynamicColumn
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Type Type { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
