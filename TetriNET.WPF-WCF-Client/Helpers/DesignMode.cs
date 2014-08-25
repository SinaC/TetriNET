using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class DesignMode
    {
        #region DesignMode detection, thanks to Galasoft

        private static bool? _isInDesignMode;

        [SuppressMessage(
            "Microsoft.Security",
            "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "The security risk here is neglectible.")]
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                            .FromProperty(prop, typeof(FrameworkElement))
                            .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

        #endregion
    }
}
