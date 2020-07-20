using DbManager.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbManager.Logic.Interfaces.ViewInterfaces
{
    public interface IUploadNewVersionView : IDisposable
    {
        UploadNewVersionModel Model { get; set; }
        ICommand PauseCommand { get; set; }
        void ShowDialog();
        void CloseDialog();
    }
}
