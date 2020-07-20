using DbManager.Logic;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace DbManager
{
    public partial class Login : Form, ILoginView
    {
        private LoginModel model;

        public Login()
        {
            InitializeComponent();
        }
        private void ClearBindings()
        {
            textBoxLogin.DataBindings.Clear();
            textBoxPassword.DataBindings.Clear();
        }
        private void SetBindings()
        {
            textBoxLogin.DataBindings.Add("Text", Model, nameof(LoginModel.Username), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxPassword.DataBindings.Add("Text", Model, nameof(LoginModel.Password), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }

        public ICommand LoginCommand { get; set; }
        public LoginModel Model
        {
            get => model; 
            set {
                model = value;
                ClearBindings();
                SetBindings();
            }
        }
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            LoginCommand.Execute(null);
        }
        void ILoginView.ShowDialog()
        {
            ShowDialog();
        }
        void ILoginView.CloseDialog()
        {
            Close();
        }
    }
}
