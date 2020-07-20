using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class LoginModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        public string Username { get => _username; set { _username = value; OnPropertyChange(nameof(Username)); } }
        public string Password { get => _password; set { _password = value; OnPropertyChange(nameof(Password)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
