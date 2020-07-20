using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Interfaces
{
    public interface INetworkPathInfo
    {
        string GetCredentialsName();

        string GetNetworkPath();
    }
}
