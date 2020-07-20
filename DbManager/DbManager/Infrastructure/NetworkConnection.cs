using DbManager.Logic.Connection;
using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Infrastructure
{
    public class NetworkConnection : INetworkConnection
    {
        public IDisposable Connect(string networkPath, string userLogin, string userPassword)
        {
            if (string.IsNullOrWhiteSpace(userLogin))
                throw new ArgumentNullException(nameof(userLogin));

            if (string.IsNullOrWhiteSpace(userPassword))
                throw new ArgumentNullException(nameof(userPassword));

            NetworkCredential credentials = new NetworkCredential(userLogin, userPassword);
            return new ConnectToSharedFolder(networkPath, credentials);
        }

        public bool TryConnect(string networkPath, string userLogin, string userPassword)
        {
            try
            {
                using (var disposable = Connect(networkPath, userLogin, userPassword))
                {
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
