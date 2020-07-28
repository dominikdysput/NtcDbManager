using DbManager.Logic;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
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

namespace DbManager.View
{
    public partial class DownloadSelectedVersion : Form, IDownloadSelectedVersionView
    {
        private DownloadSelectedVersionModel model;
        public DownloadSelectedVersion()
        {
            InitializeComponent();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            SynchronizationContext = SynchronizationContext.Current;
        }
        public DownloadSelectedVersionModel Model
        {
            get => model;
            set
            {
                model = value;
                SetBindings();
            }
        }
        public SynchronizationContext SynchronizationContext { get; }
        private void SetBindings()
        {
            progressBar1.DataBindings.Add("Value", Model, nameof(DownloadSelectedVersionModel.StatusProgressbar), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            labelProcessing.DataBindings.Add("Text", Model, nameof(DownloadSelectedVersionModel.DownloadStatus), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }
        public ICommand PauseCommand { get; set; }
        public void CloseDialog()
        {
            Close();
        }
        private  void buttonPause_Click(object sender, EventArgs e)
        {
            PauseCommand.Execute(null);
        }
        void IDownloadSelectedVersionView.ShowDialog()
        {
            Show();
        }
    }
}