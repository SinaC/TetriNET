using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class ComboBoxExtensionMethods
    {
        public static void SetWidthFromItems(this ComboBox comboBox)
        {
            double comboBoxWidth = 19;// comboBox.DesiredSize.Width;

            // Create the peer and provider to expand the comboBox in code behind. 
            ComboBoxAutomationPeer peer = new ComboBoxAutomationPeer(comboBox);
            IExpandCollapseProvider provider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);
            EventHandler eventHandler = null;
            eventHandler = delegate
            {
                if (comboBox.IsDropDownOpen &&
                    comboBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    double width = 0;
                    foreach (var item in comboBox.Items)
                    {
                        ComboBoxItem comboBoxItem = comboBox.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                        if (comboBoxItem != null)
                        {
                            comboBoxItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            if (comboBoxItem.DesiredSize.Width > width)
                            {
                                width = comboBoxItem.DesiredSize.Width;
                            }
                        }
                    }
                    comboBox.Width = comboBoxWidth + width;
                    // Remove the event handler. 
                    comboBox.ItemContainerGenerator.StatusChanged -= eventHandler;
                    comboBox.DropDownOpened -= eventHandler;
                    if (provider != null)
                        provider.Collapse();
                }
            };
            comboBox.ItemContainerGenerator.StatusChanged += eventHandler;
            comboBox.DropDownOpened += eventHandler;
            // Expand the comboBox to generate all its ComboBoxItem's. 
            if (provider != null)
                provider.Expand();
        }
    }
}
