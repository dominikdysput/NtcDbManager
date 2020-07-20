using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Infrastructure
{
    public class FormFactory<T> : IFormFactory<T> where T : class
     {
        private readonly IIoc _ioc;

        public FormFactory(IIoc ioc)
        {
            _ioc = ioc;
        }
        public T GetForm()
        {
            return _ioc.GetInstance<T>();
        }
        public IDatabasesListView CreateDatabasesListForm()
        {
            return _ioc.GetInstance<IDatabasesListView>();
        }
    }
}
