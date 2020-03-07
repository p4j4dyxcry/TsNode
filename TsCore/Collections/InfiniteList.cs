using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TsCore.Collections
{
    public class InfiniteList<T> :  IList<T>
    {
        private readonly List<T> _list;
        private int _iterator = -1;

        public InfiniteList()
        {
            _list = new List<T>();
        }

        public InfiniteList(IEnumerable<T> items)
        {
            _list = items.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public T Yield()
        {
            if (this.Any() is false)
                return default(T);

            _iterator++;

            if (_iterator >= Count)
                _iterator = 0;
            return this[_iterator];
        }
    }
}
