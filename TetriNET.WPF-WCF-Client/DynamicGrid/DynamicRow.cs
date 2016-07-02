using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TetriNET.WPF_WCF_Client.DynamicGrid
{
    public class DynamicRow : DynamicObject, INotifyPropertyChanged//, IDataErrorInfo
    {
        private readonly Dictionary<string, object> _dynamicProperties;
        //private readonly Dictionary<string, bool> _dynamicValidities;

        public DynamicRow()
        {
            _dynamicProperties = new Dictionary<string, object>();
            //_dynamicValidities = new Dictionary<string, bool>();
        }

        public DynamicRow(params KeyValuePair<string, object>[] propertyNames)
        {
            _dynamicProperties = propertyNames.ToDictionary(s => s.Key, s => s.Value);
            //_dynamicValidities = propertyNames.ToDictionary(s => s.Key, s => true);
        }

        public DynamicRow(IEnumerable<KeyValuePair<string, object>> propertyNames)
            : this(propertyNames.ToArray())
        {
        }

        public DynamicRow(params Tuple<string, object>[] propertyNames)
        {
            _dynamicProperties = propertyNames.ToDictionary(x => x.Item1, x => x.Item2);
            //_dynamicValidities = propertyNames.ToDictionary(s => s.Item1, s => true);
        }

        public DynamicRow(IEnumerable<Tuple<string, object>> propertyNames)
            : this(propertyNames.ToArray())
        {
        }

        public bool TryAddProperty(string propertyName, object propertyValue)
        {
            if (_dynamicProperties.ContainsKey(propertyName))
                return false;
            _dynamicProperties.Add(propertyName, propertyValue);
            //_dynamicValidities.Add(propertyName, true);
            OnPropertyChanged(propertyName);
            return true;
        }

        public bool TryGetProperty(string propertyName, out object propertyValue)
        {
            propertyValue = null;
            if (_dynamicProperties.ContainsKey(propertyName))
            {
                propertyValue = _dynamicProperties[propertyName];
                return true;
            }
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // TODO: type checking
            if (_dynamicProperties.ContainsKey(binder.Name))
            {
                if (_dynamicProperties[binder.Name].GetType() != value.GetType())
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(_dynamicProperties[binder.Name].GetType());
                    try
                    {
                        object converted = converter.ConvertFrom(value);
                        _dynamicProperties[binder.Name] = converted;
                        //_dynamicValidities[binder.Name] = true;
                        OnPropertyChanged(binder.Name);
                    }
                    // TODO: should notify row error provider
                    catch (FormatException ex)
                    {
                        //_dynamicValidities[binder.Name] = false;
                        OnPropertyChanged(binder.Name);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        //_dynamicValidities[binder.Name] = false;
                        OnPropertyChanged(binder.Name);
                        return false;
                    }
                }
                else
                {
                    _dynamicProperties[binder.Name] = value;
                    //_dynamicValidities[binder.Name] = true;
                    OnPropertyChanged(binder.Name);
                }

                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dynamicProperties.ContainsKey(binder.Name))
            {
                result = _dynamicProperties[binder.Name];
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dynamicProperties.Keys.ToArray();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            MemberExpression body = propertyExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException(@"Invalid argument", nameof(propertyExpression));

            PropertyInfo property = body.Member as PropertyInfo;

            if (property == null)
                throw new ArgumentException(@"Argument is not a property", nameof(propertyExpression));

            return property.Name;
        }

        protected bool Set<T>(Expression<Func<T>> selectorExpression, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;
            string propertyName = GetPropertyName(selectorExpression);
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        //#region IDataErrorInfo

        //public string this[string columnName]
        //{
        //    get { return _dynamicValidities[columnName] ? null : "Error"; }
        //}

        //public string Error { get { return String.Empty; } }

        //#endregion
    }
}
