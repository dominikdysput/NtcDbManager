using DbManager.Logic.Interfaces.ViewInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Interfaces
{
    public interface IFormFactory<T> where T : class
    {
        T GetForm();
        IDatabasesListView CreateDatabasesListForm();
    }
}
