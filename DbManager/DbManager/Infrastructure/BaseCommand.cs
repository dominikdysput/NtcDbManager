using System;
using System.Windows.Input;

namespace DbManager.Infrastructure
{
    class BaseCommand : ICommand
    {
        private readonly Action _action;
        private readonly Action<object> _parametrizedAction;

        public event EventHandler CanExecuteChanged;

        public BaseCommand(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        public BaseCommand(Action<object> action)
        {
            _parametrizedAction = action ?? throw new ArgumentNullException(nameof(action));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                if (_action != null)
                {
                    _action();
                } 
                else
                {
                    _parametrizedAction(parameter);
                }    
            }
        }
    }
}
