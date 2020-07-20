using DbManager.Infrastructure;
using DbManager.Logic.Dtos;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using Meziantou.Framework.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic.Presenters
{
    public class LoginPresenter
    {
        private ILoginView _view;
        private readonly IUserAccessCredentials _userAccess;
        private readonly IFormFactory<ILoginView> _formFactory;
        private readonly INetworkPathInfo _networkPathInfo;
        private readonly INetworkConnection _networkConnection;

        public LoginPresenter(IUserAccessCredentials userAccess, IFormFactory<ILoginView> formFactory, INetworkPathInfo networkPathInfo,
            INetworkConnection networkConnection)
        {
            _formFactory = formFactory;
            _networkPathInfo = networkPathInfo;
            _networkConnection = networkConnection;
            _userAccess = userAccess;
        }

        private void BindCommands()
        {
            _view.LoginCommand = new BaseCommand(new Action(() =>
            {
                if (_networkConnection.TryConnect(_networkPathInfo.GetNetworkPath(), _view.Model.Username, _view.Model.Password))
                {
                    var permissions = new CredentialsDto()
                    {
                        NetworkPath = _networkPathInfo.GetCredentialsName(),
                        Login = _view.Model.Username,
                        Password = _view.Model.Password
                    };
                    _userAccess.WriteCredentials(permissions);
                    _view.CloseDialog();
                }
                else
                {
                    MessageBox.Show("Fill all fields or enter the correct data");
                }
            }));
        }

        public void Run()
        {
            using (_view = _formFactory.GetForm())
            {
                _view.Model = new Model.LoginModel();
                BindCommands();
                _view.ShowDialog();
            }
        }
        public bool CheckPermissions()
        {
            var cred = _userAccess.ReadCredentials();
            if (cred == null) 
                return false;
            try
            {
                _networkConnection.TryConnect(_networkPathInfo.GetNetworkPath(), cred.Login, cred.Password);
            }
            catch
            {
                return false;
            }
            
            return true;
        }
    }
}
