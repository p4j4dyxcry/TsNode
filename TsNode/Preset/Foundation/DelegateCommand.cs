using System;
using System.Windows.Input;

namespace TsNode.Preset.Foundation
{
    internal class DelegateCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<T> _action;
        public DelegateCommand(Action<T> action) => _action = action;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _action.Invoke((T) parameter);

        public void RaiseCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}