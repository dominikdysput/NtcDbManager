using DbManager.Infrastructure;
using DbManager.Logic;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
using DbManager.Logic.Presenters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace DbManager.View
{
    public partial class DatabaseDetails : Form, IDatabaseDetailsView
    {
        private DatabaseDetailsModel model;
        public DatabaseDetails()
        {
            InitializeComponent();
            dataGridViewDetails.Columns["Id"].Visible = false;
            dataGridViewDetails.Columns["PathToFile"].Visible = false;
            dataGridViewDetails.Columns["Checksum"].Visible = false;
        }
        public DatabaseDetailsModel Model
        {
            get => model;
            set
            {
                model = value;
                SetBindings();
            }
        }
        public IAsyncCommand Upload { get; set; }
        public IAsyncCommand Download { get; set; }

        private void SetBindings()
        {
            textBoxCompany.DataBindings.Add("Text", Model, nameof(DatabaseDetailsModel.Company), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxDbName.DataBindings.Add("Text", Model, nameof(DatabaseDetailsModel.DatabaseName), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            textBoxTags.DataBindings.Add("Text", Model, nameof(DatabaseDetailsModel.Tags), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
            dataGridViewDetails.DataBindings.Add("DataSource", Model, nameof(DatabaseDetailsModel.DataTable), false, DataSourceUpdateMode.OnPropertyChanged, string.Empty);
        }
        private void UploadDbButton_Click(object sender, EventArgs e)
        {
            Upload.Execute();
        }
        private void dataGridViewDetails_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            Model.PathToSource = dataGridViewDetails.Rows[e.RowIndex].Cells[3].Value.ToString();
            Model.Checksum = dataGridViewDetails.Rows[e.RowIndex].Cells[4].Value.ToString();
            Download.Execute();
        }
        void IDatabaseDetailsView.ShowDialog()
        {
            dataGridViewDetails.AutoGenerateColumns = false;
            ShowDialog();
        }
        public void CloseDialog()
        {
            Close();
        }
    }
}
