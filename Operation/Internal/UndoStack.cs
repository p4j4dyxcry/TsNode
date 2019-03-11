using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TsGui.Operation.Internal
{
    internal class UndoStack<T> : IStack<T>
    {
        private readonly IStack<T> _undoStack;
        private readonly IStack<T> _redoStack;

        public bool CanUndo => _undoStack.Any();
        public bool CanRedo => _redoStack.Any();

        public int Count => _undoStack.Count();

        public UndoStack(int capacity)
        {
            _undoStack = new CapacityStack<T>(capacity);
            _redoStack = new CapacityStack<T>(capacity);
        }

        public T Undo()
        {
            return _redoStack.Push(_undoStack.Pop());
        }

        public T Redo()
        {
            return  _undoStack.Push(_redoStack.Pop());
        }

        public T Peek()
        {
            if (CanUndo is false)
                return default(T);

            return _undoStack.Peek();
        }

        public T Push(T item)
        {
            _redoStack.Clear();
            return _undoStack.Push(item);
        }

        public T Pop()
        {
            _redoStack.Clear();

            if (CanUndo is false)
                return default(T);

            return _undoStack.Pop();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _undoStack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> RedoStack => _redoStack;
    }
}
