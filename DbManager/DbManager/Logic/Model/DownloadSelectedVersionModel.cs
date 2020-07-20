using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class DownloadSelectedVersionModel : INotifyPropertyChanged
    {
        private int _statusProgressbar;
        private string _downloadStatus;
        public string DownloadStatus { get => _downloadStatus; set { _downloadStatus = value; OnPropertyChange(nameof(DownloadStatus)); } }
        public int StatusProgressbar { get => _statusProgressbar; set { _statusProgressbar = value; OnPropertyChange(nameof(StatusProgressbar)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
