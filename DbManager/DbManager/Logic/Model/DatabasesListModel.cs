using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class DatabasesListModel : INotifyPropertyChanged
    {
        private int _statusProgressbar;
        private string _updateStatus;
        private string _databaseName;
        private string _company;
        private string _tags;
        private string _pathToFile;
        private string _findByInput;
        private readonly SynchronizationContext _ctx;
        public DatabasesListModel()
        {

        }
        public DatabasesListModel(SynchronizationContext ctx)
        {
            _ctx = ctx;
        }
     
        private int _comboboxSelectedItem;
        private DataTable _dataTable;
        

        public int StatusProgressbar { get => _statusProgressbar; set { _statusProgressbar = value; OnPropertyChange(nameof(StatusProgressbar)); } }
        public string DatabaseName { get => _databaseName; set { _databaseName = value; OnPropertyChange(nameof(DatabaseName)); } }
        public string PathToFile { get => _pathToFile; set { _pathToFile = value; OnPropertyChange(nameof(PathToFile)); } }
        public string Company { get => _company; set { _company = value; OnPropertyChange(nameof(Company)); } }
        public string FindByInput { get => _findByInput; set { _findByInput = value; OnPropertyChange(nameof(FindByInput)); } }
        public string UpdateStatus { get => _updateStatus; set { _updateStatus = value; OnPropertyChange(nameof(UpdateStatus)); } }
        public int ComboboxSelectedItem { get => _comboboxSelectedItem; set { _comboboxSelectedItem = value; OnPropertyChange(nameof(ComboboxSelectedItem)); } }
        public string Tags { get => _tags; set { _tags = value; OnPropertyChange(nameof(Tags)); } }
        public DataTable DataTable { get => _dataTable; set { _dataTable = value; OnPropertyChange(nameof(DataTable)); } } 

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChange(string name)
        {
            if (_ctx != null)
            {
                _ctx.Post((s) =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }, null);
                return;
            }
        }
    }
}
