using DbManager.Infrastructure;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic.Presenters
{
    public class DownloadSelectedVersionPresenter
    {
        private string _pathToSource = string.Empty;
        private string _checksum = string.Empty;
        private IDownloadSelectedVersionView _view;
        private readonly IFormFactory<IDownloadSelectedVersionView> _formFactory;
        private readonly IFileManager _fileManager;
        private readonly IMessageService _messageService;
        private CancellationTokenSource _cancellationTokenSource;

        public DownloadSelectedVersionPresenter(IFileManager fileManager, IFormFactory<IDownloadSelectedVersionView> formFactory,
            IMessageService messageService)
        {
            _fileManager = fileManager;
            _formFactory = formFactory;
            _messageService = messageService;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        public void Init(string pathToSource, string checksum)
        {
            _pathToSource = pathToSource;
            _checksum = checksum;
        }
        private void BindCommands()
        {
            _view.PauseCommand = new BaseCommand(new Action(() =>
            {
                _cancellationTokenSource.Cancel();
            }));
        }
        private async Task ShowDialogSelectPath()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var _pathToDownloadLoc = _messageService.ShowOpenFolderDialog();
                if (string.IsNullOrEmpty(_pathToDownloadLoc))
                {
                    return;
                }
                var resumableFileManager = _fileManager as IResumableFileManager;
                resumableFileManager.ProgressbarChangedDelegate = DownloadProgressChangedHandler;
                resumableFileManager.StatusChangedDelegate = DownloadStatusChangedHandler;
                resumableFileManager.ProcessingFinishedDelegate = DownloadFinshedDelegateHandler;

                if (resumableFileManager != null && resumableFileManager.CheckInfoFileIsAlreadyDownloaded(_pathToDownloadLoc))
                {
                    bool userDecision = _messageService.CheckUserWantsToResumeDownload();
                    if (userDecision)
                    {
                        await resumableFileManager.ResumeDownload(_pathToSource, _pathToDownloadLoc, _checksum, true, _cancellationTokenSource.Token);
                        return;
                    }
                }
                await _fileManager.Download(_pathToSource, _pathToDownloadLoc, _checksum, false,  _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DownloadProgressChangedHandler(long totalBytes, long sizeOfFile)
        {
            var percent = (int)((double)totalBytes / sizeOfFile * 100);
            _view.Model.StatusProgressbar = percent;
        }
        private void DownloadFinshedDelegateHandler(bool status, Exception exception)
        {
            if (!status)
            {
                _view.CloseDialog();
                try
                {
                    throw exception;
                }
                catch (System.OperationCanceledException)
                {
                    MessageBox.Show("Downloading canceled");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Download finished successfully");
            }
        }
        private void DownloadStatusChangedHandler(long totalBytes, long sizeOfFile, long processingSpeed)
        {
            _view.Model.DownloadStatus = $"{totalBytes / 1048576}/{sizeOfFile / 1048576}  MB  {processingSpeed / 1048576} MB/s";
        }
        public void Run()
        {
            using (_view = _formFactory.GetForm())
            {
                BindCommands();
                _view.Model = new Model.DownloadSelectedVersionModel();
                _cancellationTokenSource = new CancellationTokenSource();
                ShowDialogSelectPath();
                _view.ShowDialog();
            }
        }
    }
}
