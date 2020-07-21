using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Infrastructure
{
    class AsyncCommand : IAsyncCommand
    {
        private Func<Task> _func;

        public AsyncCommand(Func<Task> func)
        {
            _func = func;
        }
        public bool CanExecute()
        {
            return true;
        }
        public Task Execute()
        {
            return _func();
        }

    }
}
