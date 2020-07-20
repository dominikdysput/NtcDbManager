using DbManager.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbManager.Logic.Interfaces.ViewInterfaces
{
    public interface ILoginView: IDisposable
    {
        ICommand LoginCommand { get; set; }
        LoginModel Model { get; set; }
        void ShowDialog();
        void CloseDialog();
    }
}
