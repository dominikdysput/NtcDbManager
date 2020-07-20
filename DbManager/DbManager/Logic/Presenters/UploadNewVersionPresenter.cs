﻿using DbManager.Infrastructure;
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
    public class UploadNewVersionPresenter
    {
        private string _pathToFile;
        private string _networkPath = Properties.Settings.Default.NetworkPath + @"\DD_Test\";
        private int _id;
        private IUploadNewVersionView _view;
        private readonly IFormFactory<IUploadNewVersionView> _formFactory;
        private readonly IIoc _ioc;
        private readonly IMetaData _metaData;
        private readonly IChecksum _checksum;
        private readonly IFileManager _fileManager;
        private readonly IMessageService _messageService;
        private readonly IFileValidator _fileValidator;
        private CancellationTokenSource _cancellationTokenSource;
        public UploadNewVersionPresenter(IMetaData metaData, IChecksum checksum, IFileManager fileManager,
            IMessageService messageService, IFileValidator fileValidator, IFormFactory<IUploadNewVersionView> formFactory, IIoc ioc)
        {
            _formFactory = formFactory;
            _ioc = ioc;
            _messageService = messageService;
            _fileValidator = fileValidator;
            _checksum = checksum;
            _metaData = metaData;
            _fileManager = fileManager;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        private void BindCommands()
        {
            _view.PauseCommand = new BaseCommand(new Action(() =>
            {
                _cancellationTokenSource.Cancel();
            }));
        }
        public void Init(int id)
        {
            _id = id;
        }
        private void ShowDialogSelectPath()
        {
            _pathToFile = string.Empty;
            _pathToFile = _messageService.ShowOpenFileDialog();
            if (string.IsNullOrEmpty(_pathToFile))
                return;
        }
        private async Task StartUpload()
        {
            try
            {
                MessageBox.Show("Processing...");
                var checksumForNewFile = _checksum.CalculateChecksum(_pathToFile);
                var resumableFileManager = _fileManager as IResumableFileManager;
                string targetFile = string.Empty;

                resumableFileManager.ProgressbarChangedDelegate = UploadProgressChangedHandler;
                resumableFileManager.StatusChangedDelegate = UploadStatusChangedHandler;
                resumableFileManager.ProcessingFinishedDelegate = UploadFinshedDelegateHandler;

                if (resumableFileManager != null && resumableFileManager.CheckInfoFileIsAlreadyDownloaded(_networkPath))
                {
                    bool userDecision = _messageService.CheckUserWantsToResumeUpload();
                    if (userDecision)
                    {
                        targetFile = await resumableFileManager.ResumeUpload(_pathToFile, _networkPath, checksumForNewFile, true, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        targetFile = await resumableFileManager.Upload(_pathToFile, _networkPath, checksumForNewFile, true, _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    if (!_fileValidator.CheckVersionAlreadyExists(_id, checksumForNewFile))
                        targetFile = await resumableFileManager.Upload(_pathToFile, _networkPath, checksumForNewFile, true, _cancellationTokenSource.Token);
                    else
                        throw new Exception("This version already exists");
                }

                if (!string.IsNullOrEmpty(targetFile))
                    _metaData.WriteDetails(_id, targetFile, checksumForNewFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void UploadFinshedDelegateHandler(bool status, Exception exception)
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
                    MessageBox.Show("Uploading canceled");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Uploading finished successfully");
            }
        }
        private void UploadProgressChangedHandler(long totalBytes, long sizeOfFile)
        {
            var percent = (int)((double)totalBytes / sizeOfFile * 100);
            _view.Model.StatusProgressbar = percent;
        }
        private void UploadStatusChangedHandler(long totalBytes, long sizeOfFile, long processingSpeed)
        {
            _view.Model.UploadStatus = $"{totalBytes / 1048576}/{sizeOfFile / 1048576}  MB  {processingSpeed / 1048576} MB/s";
        }
        public async Task Run()
        {
            using (_view = _formFactory.GetForm())
            {
                BindCommands();
                _view.Model = new Model.UploadNewVersionModel();
                _cancellationTokenSource = new CancellationTokenSource();
                ShowDialogSelectPath();
                _view.ShowDialog();
                await StartUpload();

            }
        }
    }
}