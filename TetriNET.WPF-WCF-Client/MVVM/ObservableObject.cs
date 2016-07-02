using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TetriNET.WPF_WCF_Client.MVVM
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public const double Tolerance = 0.00001;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
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

        protected bool Set(Expression<Func<double>> selectorExpression, ref double field, double newValue)
        {
            if (Math.Abs(field - newValue) < Tolerance)
                return false;
            field = newValue;
            string propertyName = GetPropertyName(selectorExpression);
            OnPropertyChanged(propertyName);
            return true;
        }

        //protected bool Set<T>(string propertyName, ref T field, T newValue)
        //{
        //    if (EqualityComparer<T>.Default.Equals(field, newValue))
        //        return false;
        //    field = newValue;
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}

        //protected bool Set(string propertyName, ref double field, double newValue)
        //{
        //    if (Math.Abs(field - newValue) < Tolerance)
        //        return false;
        //    field = newValue;
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}
    }
}
