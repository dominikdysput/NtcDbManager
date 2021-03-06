﻿using DbManager.Infrastructure;
using DbManager.Logic;
using DbManager.Logic.Connection;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
using DbManager.Logic.Presenters;
using DbManager.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace DbManager
{
    partial class DatabasesList : Form, IDatabasesListView
    {
        private readonly IMessageService _messageService;
        private DatabasesListModel model;
        public DatabasesList(IMessageService messageService)
        {
            InitializeComponent();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns["Id"].Visible = false;
            SynchronizationContext = SynchronizationContext.Current;
            _messageService = messageService;
        }
        public DatabasesListModel Model
        {
            get => model;
            set
            {
                model = value;
                SetBindings();
            }
        }
        public ICommand PauseCommand { get; set; }
        public IAsyncCommand Upload { get; set; }
        public ICommand Find { get; set; }
        public ICommand GetInfoDatabaseCommand { get; set; }
        public SynchronizationContext SynchronizationContext { get; }
        private void SetBindings()
        {
            textBoxCompany.DataBindings.Add("Text", Model, nameof(DatabasesListModel.Company), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxDbName.DataBindings.Add("Text", Model, nameof(DatabasesListModel.DatabaseName), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxTags.DataBindings.Add("Text", Model, nameof(DatabasesListModel.Tags), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            progressBar1.DataBindings.Add("Value", Model, nameof(DatabasesListModel.StatusProgressbar), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            dataGridView1.DataBindings.Add("DataSource", Model, nameof(DatabasesListModel.DataTable), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            comboBox1.DataBindings.Add("SelectedIndex", Model, nameof(DatabasesListModel.ComboboxSelectedItem), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxFindByInput.DataBindings.Add("Text", Model, nameof(DatabasesListModel.FindByInput), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            labelProcessing.DataBindings.Add("Text", Model, nameof(DatabasesListModel.UpdateStatus), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }
        private async void buttonUpload_Click(object sender, EventArgs e)
        {
            var pathToFile = _messageService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(pathToFile))
                Model.PathToFile = pathToFile;
            Model.Company = textBoxCompany.Text;
            Model.DatabaseName = textBoxDbName.Text;
            Model.Tags = textBoxTags.Text;
            await Upload.Execute();
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            var id = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            GetInfoDatabaseCommand.Execute(id);
        }
        private void buttonStop_Click(object sender, EventArgs e)
        {
            PauseCommand.Execute(null);
        }
        void IDatabasesListView.ShowDialog()
        {
            ShowDialog();
        }
        void IDatabasesListView.CloseDialog()
        {
            Close();
        }
        bool IDatabasesListView.CheckTextboxesEmpty()
        {
            if (string.IsNullOrEmpty(Model.Company) || string.IsNullOrEmpty(Model.DatabaseName) || string.IsNullOrEmpty(Model.Tags))
            {
                MessageBox.Show("Fill all fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            if(string.IsNullOrEmpty(Model.PathToFile))
            {
                MessageBox.Show("Choose file to upload", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            ChangeButtonsEnabledStatus();
            return false;
        }
        void IDatabasesListView.ClearModel()
        {
            Model.Company = "";
            Model.DatabaseName = "";
            Model.Tags = "";
            Model.StatusProgressbar = 0;
            Model.UpdateStatus = "";
            ChangeButtonsEnabledStatus();
        }
        private void ChangeButtonsEnabledStatus()
        {
            textBoxCompany.Enabled = !textBoxCompany.Enabled;
            textBoxDbName.Enabled = !textBoxDbName.Enabled;
            textBoxTags.Enabled = !textBoxTags.Enabled;
            buttonUpload.Enabled = !buttonUpload.Enabled;
            buttonStop.Enabled = !buttonStop.Enabled;
            labelProcessing.Visible = !labelProcessing.Visible;
            progressBar1.Enabled = !progressBar1.Enabled;
            labelStatus.Visible = !labelStatus.Visible;
        }
        private void DatabasesList_Load_1(object sender, EventArgs e)
        {
            Model.ComboboxSelectedItem = 0;
        }
        private void textBoxFindByInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Find.Execute(null);
            }
        }

    }
}
