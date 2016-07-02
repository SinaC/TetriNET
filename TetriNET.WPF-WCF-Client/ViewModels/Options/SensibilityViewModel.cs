using System;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class SensibilityViewModel : ObservableObject
    {
        private string PropertyName { get; }

        private bool _isActivated;
        public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                if (_isActivated != value)
                {
                    _isActivated = value;
                    if (!String.IsNullOrEmpty(PropertyName))
                    {
                        Settings.Default[PropertyName + "Activated"] = value;
                        Settings.Default.Save();
                    }
                    OnPropertyChanged();
                }
            }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    if (!String.IsNullOrEmpty(PropertyName))
                    {
                        Settings.Default[PropertyName] = value;
                        Settings.Default.Save();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public SensibilityViewModel()
        {
        }

        public SensibilityViewModel(string propertyName)
        {
            PropertyName = propertyName;
            _isActivated = (bool)Settings.Default[PropertyName + "Activated"];
            _value = (int)Settings.Default[PropertyName];
        }
    }
}
