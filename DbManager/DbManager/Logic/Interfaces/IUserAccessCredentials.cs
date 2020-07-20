using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DbManager.Logic.Dtos;
using Meziantou.Framework.Win32;

namespace DbManager.Logic.Interfaces
{
    public interface IUserAccessCredentials
    {
        void WriteCredentials(CredentialsDto credentialsForUserCredentialDto);
        CredentialsDto ReadCredentials();
    }
}
