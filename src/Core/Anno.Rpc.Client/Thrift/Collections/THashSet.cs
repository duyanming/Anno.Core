using System;
using System.Collections;
using System.Collections.Generic;

namespace Thrift.Collections
{
    [Serializable]
    public class THashSet<T> : ICollection<T>
    {
        HashSet<T> set = new HashSet<T>();
        public Int32 Count => set.Count;

        public Boolean IsReadOnly => false;

        public void Add(T item) => set.Add(item);

        public void Clear() => set.Clear();

        public Boolean Contains(T item) => set.Contains(item);

        public void CopyTo(T[] array, Int32 arrayIndex) => set.CopyTo(array, arrayIndex);

        public IEnumerator GetEnumerator() => set.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)set).GetEnumerator();

        public Boolean Remove(T item) => set.Remove(item);
    }
}