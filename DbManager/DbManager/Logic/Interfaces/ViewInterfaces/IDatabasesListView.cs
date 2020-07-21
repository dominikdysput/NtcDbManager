using DbManager.Infrastructure;
using DbManager.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbManager.Logic.Interfaces.ViewInterfaces
{
    public interface IDatabasesListView : IDisposable
    {
        SynchronizationContext SynchronizationContext { get; }
        DatabasesListModel Model { get; set; }
        ICommand PauseCommand { get; set; }
        IAsyncCommand Upload { get; set; }
        ICommand Find { get; set; }
        ICommand GetInfoDatabaseCommand{get;set;}
        void ShowDialog();
        void CloseDialog();
        bool CheckTextboxesEmpty();
        void ClearModel();
    }
}
