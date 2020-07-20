using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class DatabaseDetailsModel : INotifyPropertyChanged
    {
        private string databaseName;
        private string company;
        private string tags;
        private string pathToSource;
        private string checksum;

        private DataTable dataTable;
        public DataTable DataTable { get => dataTable; set { dataTable = value; OnPropertyChange(nameof(DataTable)); } }
        public string DatabaseName { get => databaseName; set { databaseName = value; OnPropertyChange(nameof(DatabaseName)); } }
        public string Company { get => company; set { company = value; OnPropertyChange(nameof(Company)); } }
        public string Tags { get => tags; set { tags = value; OnPropertyChange(nameof(Tags)); } }
        public string PathToSource { get => pathToSource; set { pathToSource = value; OnPropertyChange(nameof(PathToSource)); } }
        public string Checksum { get => checksum; set { checksum = value; OnPropertyChange(nameof(Checksum)); } }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
