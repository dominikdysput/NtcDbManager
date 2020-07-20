using DbManager.Logic.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Extensions
{
    static class Win32CredensialsExtensions
    {
        public static CredentialsDto ToDto(this Meziantou.Framework.Win32.Credential credential)
        {
            if (credential == null)
                return null;
            return new CredentialsDto()
            {
                Login = credential.UserName,
                Password = credential.Password,
                NetworkPath = credential.ApplicationName
            };
        }
    }
}
