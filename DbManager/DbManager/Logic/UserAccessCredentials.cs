using DbManager.Logic.Interfaces;
using Meziantou.Framework.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal; // WindowsImpersonationContext
using System.Security.Permissions; // PermissionSetAttribute
using CredentialManagement;
using System.Diagnostics;
using Windows;
using System.Net;
using System.IO;
using DbManager.Logic.Dtos;
using DbManager.Extensions;

namespace DbManager.Logic
{
    public class UserAccessCredentials : IUserAccessCredentials
    {
        private readonly INetworkPathInfo _networkPathInfo;
        public UserAccessCredentials(INetworkPathInfo networkPathInfo)
        {
            _networkPathInfo = networkPathInfo;
        }
        public CredentialsDto ReadCredentials()
        {
            var credentials = CredentialManager.ReadCredential(_networkPathInfo.GetCredentialsName());
            return credentials.ToDto();
        }
        public void WriteCredentials(CredentialsDto credentialsForUserCredentialDto)
        {
            var userLogin = credentialsForUserCredentialDto.Login;
            var userPassword = credentialsForUserCredentialDto.Password;
            var networkPath = credentialsForUserCredentialDto.NetworkPath;
            CredentialManager.WriteCredential(networkPath, userLogin, userPassword, CredentialPersistence.LocalMachine);
        }

    }
}
