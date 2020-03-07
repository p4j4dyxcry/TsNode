using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TsCore.Operation.Internal;

namespace TsCore.Operation
{
    /// <summary>
    /// 標準的なオペレーションコントローラ
    /// </summary>
    public class OperationController : IOperationController 
    {
        private readonly UndoStack<IOperation> _undoStack;
        public bool CanUndo => _undoStack.CanUndo;
        public bool CanRedo => _undoStack.CanRedo;

        public OperationController(int capacity)
        {
            Debug.Assert(capacity > 0 , ErrorMessages.InvalidOperation);
            _undoStack = new UndoStack<IOperation>(capacity);
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            PreStackChanged();
            _undoStack.Undo().Rollback();
            OnStackChanged(OperationStackChangedEvent.Undo);
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            PreStackChanged();
            _undoStack.Redo().RollForward();
            OnStackChanged(OperationStackChangedEvent.Redo);
        }

        public IOperation Peek()
        {
            return _undoStack.Peek();
        }

        public IOperation Pop()
        {
            PreStackChanged();
            var result = _undoStack.Pop();
            OnStackChanged(OperationStackChangedEvent.Pop);
            return result;
        }

        public IOperation Push(IOperation operation)
        {
            PreStackChanged();
            _undoStack.Push(operation);
            OnStackChanged(OperationStackChangedEvent.Push);
            return operation;
        }

        public IOperation Execute(IOperation operation)
        {
            Debug.Assert(operation != null , ErrorMessages.NotNull);
            Push(operation).RollForward();
            return operation;
        }

        public void Flush()
        {
            PreStackChanged();
            _undoStack.Clear();
            OnStackChanged(OperationStackChangedEvent.Clear);
        }

        #region PropertyChanged

        private int _preStackChangedCall;

        public IEnumerable<IOperation> RollForwardTargets => _undoStack.RedoStack.Reverse();
        public event Action<object, OperationStackChangedEventArgs> StackChanged;

        private void PreStackChanged()
        {
            //! Operationの再帰呼び出しを検知するとassert
            Debug.Assert(_preStackChangedCall == 0 , ErrorMessages.InvalidOperation);
            _preStackChangedCall++;
        }

        private void OnStackChanged(OperationStackChangedEvent eventType)
        {
            Debug.Assert(_preStackChangedCall == 1, ErrorMessages.InvalidOperation);
            _preStackChangedCall--;
            StackChanged?.Invoke(this, new OperationStackChangedEventArgs(){EventType = eventType });
        }
        #endregion

        public IEnumerable<IOperation> Operations => _undoStack;

        public bool IsOperating =>_preStackChangedCall != 0;
    }
}
