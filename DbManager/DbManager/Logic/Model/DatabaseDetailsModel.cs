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
        private string _databaseName;
        private string _company;
        private string _tags;
        private string _pathToSource;
        private string _checksum;

        private DataTable dataTable;
        public DataTable DataTable { get => dataTable; set { dataTable = value; OnPropertyChange(nameof(DataTable)); } }
        public string DatabaseName { get => _databaseName; set { _databaseName = value; OnPropertyChange(nameof(DatabaseName)); } }
        public string Company { get => _company; set { _company = value; OnPropertyChange(nameof(Company)); } }
        public string Tags { get => _tags; set { _tags = value; OnPropertyChange(nameof(Tags)); } }
        public string PathToSource { get => _pathToSource; set { _pathToSource = value; OnPropertyChange(nameof(PathToSource)); } }
        public string Checksum { get => _checksum; set { _checksum = value; OnPropertyChange(nameof(Checksum)); } }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
