using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TsCore.Operation.Internal
{
    internal interface IStack<T> : IEnumerable<T>
    {
        T Push(T item);
        T Peek();
        T Pop();
        void Clear();
    }

    internal class CapacityStack<T> : IStack<T>
    {
        private readonly LinkedList<T> _collection = new LinkedList<T>();

        public CapacityStack(int capacity) { Capacity = capacity; }

        public int Capacity { get; }

        public T Push(T item)
        {
            _collection.AddLast(item);
            if (_collection.Count > Capacity)
                _collection.RemoveFirst();
            return item;
        }
        public T Peek() => _collection.Last();

        public T Pop()
        {
            var item = _collection.Last();
            _collection.RemoveLast();
            return item;
        }

        public void Clear() => _collection.Clear();

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
