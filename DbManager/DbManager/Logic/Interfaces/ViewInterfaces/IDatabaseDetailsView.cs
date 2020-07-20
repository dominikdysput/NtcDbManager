using DbManager.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbManager.Logic.Interfaces.ViewInterfaces
{
    public interface IDatabaseDetailsView : IDisposable
    {
        DatabaseDetailsModel Model { get; set; }
        ICommand Upload { get; set; }
        ICommand Download { get; set; }
        void ShowDialog();
        void CloseDialog();
    }
}
