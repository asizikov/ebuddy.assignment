using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace eBuddy.Assignment.Utils
{
    public class SimpleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        [CanBeNull] private readonly Action _action = null;

        private readonly bool _canExecute;

        public SimpleCommand([CanBeNull] Action action, bool canExecute = true)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            if (_action != null) _action();
        }
    }
}
