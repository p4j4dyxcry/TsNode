using System;
using System.Threading;
using System.Windows.Input;

namespace TsGui.Mvvm
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T,bool> _canExecute;
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public DelegateCommand(Action<T> execute, Func<T,bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return OnCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            OnExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        protected virtual bool OnCanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) is true;
        }

        protected virtual void OnExecute(object parameter)
        {
            _execute?.Invoke((T)parameter);
        }

        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;

            if(handler is null)
                return;

            if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                _synchronizationContext.Post((o) => handler.Invoke(this, EventArgs.Empty), null);
            else
                handler.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action execute, Func<bool> canExecute = null) : base( _ => execute.Invoke() , _ => canExecute() )
        {
        }
    }
}