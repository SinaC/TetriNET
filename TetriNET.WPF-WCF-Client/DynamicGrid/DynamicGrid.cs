using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
namespace TetriNET.WPF_WCF_Client.DynamicGrid
{
    public class DynamicGrid<TRow, TColumn> : IList, ITypedList, INotifyCollectionChanged
        where TRow : DynamicObject
        where TColumn : IDynamicColumn
    {
        public IList<TRow> Rows { get; }
        public List<TColumn> Columns { get; }

        private IList UnspecializedRows => (IList)Rows;

        public DynamicGrid(IList<TRow> rows, IEnumerable<TColumn> columns)
        {
            Rows = rows;
            Columns = columns.ToList();
        }

        public void AddRow(TRow row)
        {
            Rows.Add(row);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, row));
        }

        #region ITypedList

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptor[] dynamicDescriptors;

            if (Columns.Any())
                dynamicDescriptors = Columns.Select(column => new DynamicPropertyDescriptor(column.Name, column.DisplayName, column.Type, column.IsReadOnly)).Cast<PropertyDescriptor>().ToArray();
            else
                dynamicDescriptors = new PropertyDescriptor[0];

            return new PropertyDescriptorCollection(dynamicDescriptors);
        }

        #endregion

        #region IList

        public IEnumerator GetEnumerator()
        {
            return UnspecializedRows.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            UnspecializedRows.CopyTo(array, index);
        }

        public int Count => UnspecializedRows.Count;

        public object SyncRoot => UnspecializedRows.SyncRoot;

        public bool IsSynchronized => UnspecializedRows.IsSynchronized;

        public int Add(object value)
        {
            int index = UnspecializedRows.Add(value);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            return index;
        }

        public bool Contains(object value)
        {
            return UnspecializedRows.Contains(value);
        }

        public void Clear()
        {
            UnspecializedRows.Clear();
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public int IndexOf(object value)
        {
            return UnspecializedRows.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            UnspecializedRows.Insert(index, value);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }

        public void Remove(object value)
        {
            UnspecializedRows.Remove(value);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
        }

        public void RemoveAt(int index)
        {
            object o = this[index];
            UnspecializedRows.RemoveAt(index);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, o));
        }

        public object this[int index]
        {
            get { return UnspecializedRows[index]; }
            set { UnspecializedRows[index] = value; }
        }

        public bool IsReadOnly => UnspecializedRows.IsReadOnly;

        public bool IsFixedSize => UnspecializedRows.IsFixedSize;

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        #endregion
    }
}
