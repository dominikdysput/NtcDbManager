using DbManager.Logic;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
using DbManager.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace DbManager
{
    public partial class UploadNewVersion : Form, IUploadNewVersionView
    {
        private UploadNewVersionModel model;
        public SynchronizationContext SynchronizationContext { get; }
        public UploadNewVersion()
        {
            InitializeComponent();
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            SynchronizationContext = SynchronizationContext.Current;
        }
        public UploadNewVersionModel Model
        {
            get => model;
            set
            {
                model = value;
                SetBindings();
            }
        }
        private void SetBindings()
        {
            progressBar2.DataBindings.Add("Value", Model, nameof(UploadNewVersionModel.StatusProgressbar), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            labelProcessing.DataBindings.Add("Text", Model, nameof(UploadNewVersionModel.UploadStatus), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }
        public ICommand PauseCommand { get; set; }
        private void buttonStop_Click(object sender, EventArgs e)
        {
            PauseCommand.Execute(null);
        }
        void IUploadNewVersionView.ShowDialog()
        {
            Show();
        }
        void IUploadNewVersionView.CloseDialog()
        {
            Close();
        }
    }
}
