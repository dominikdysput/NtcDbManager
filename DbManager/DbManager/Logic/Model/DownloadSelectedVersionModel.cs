using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class DownloadSelectedVersionModel : INotifyPropertyChanged
    {
        private int _statusProgressbar;
        private string _downloadStatus;
        private readonly SynchronizationContext _ctx;
        public DownloadSelectedVersionModel(SynchronizationContext ctx)
        {
            _ctx = ctx;
        }
        public string DownloadStatus { get => _downloadStatus; set { _downloadStatus = value; OnPropertyChange(nameof(DownloadStatus)); } }
        public int StatusProgressbar { get => _statusProgressbar; set { _statusProgressbar = value; OnPropertyChange(nameof(StatusProgressbar)); } }

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
