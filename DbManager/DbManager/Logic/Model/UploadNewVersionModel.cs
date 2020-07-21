using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbManager.Logic.Model
{
    public class UploadNewVersionModel : INotifyPropertyChanged
    {
        private int _statusProgressbar;
        private string _uploadStatus;
        private readonly SynchronizationContext _ctx;
        public UploadNewVersionModel(SynchronizationContext ctx)
        {
            _ctx = ctx;
        }
        public int StatusProgressbar { get => _statusProgressbar; set { _statusProgressbar = value; OnPropertyChange(nameof(StatusProgressbar)); } }
        public string UploadStatus { get => _uploadStatus; set { _uploadStatus = value; OnPropertyChange(nameof(UploadStatus)); } }

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
