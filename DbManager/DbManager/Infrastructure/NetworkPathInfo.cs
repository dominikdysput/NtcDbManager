using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Infrastructure
{
    class NetworkPathInfo : INetworkPathInfo
    {
        public string GetCredentialsName()
        {
            return $"DD_{Properties.Settings.Default.NetworkPath}";
        }
        public string GetNetworkPath()
        {
            return Properties.Settings.Default.NetworkPath;
        }
    }
}
