using System;
using System.Diagnostics;

namespace TsCore.Operation
{
    /// <summary>
    /// デリゲートオペレーション
    /// </summary>
    public class DelegateOperation : IOperation
    {
        private readonly Action _execute;
        private readonly Action _rollback;

        public DelegateOperation( Action execute , Action rollback)
        {
            Debug.Assert(execute != null);
            Debug.Assert(rollback != null);
            _execute = execute;
            _rollback = rollback;            
        }

        public string Messaage { get; set; }

        public void RollForward()
        {
            _execute.Invoke();
        }

        public void Rollback()
        {
            _rollback.Invoke();
        }
    }
    
    public class DelegateOperation<T> : IOperation
    {
        private readonly Action<T> _function;
        private readonly T _prevValue;
        private readonly T _newValue;

        public DelegateOperation(Action<T> method, T newValue , T prevValue)
        {
            _function = method;
            _prevValue = prevValue;
            _newValue = newValue;
        }

        public string Messaage { get; set; }

        public void RollForward()
        {
            _function.Invoke(_newValue);
        }

        public void Rollback()
        {
            _function.Invoke(_prevValue);
        }
    }
}