using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Interfaces
{
    public interface INetworkConnection
    {
        IDisposable Connect(string networkPath, string userLogin, string userPassword);
        bool TryConnect(string networkPath, string userLogin, string userPassword);
    }
}
