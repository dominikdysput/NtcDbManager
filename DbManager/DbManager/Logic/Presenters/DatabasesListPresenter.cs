﻿using DbManager.Infrastructure;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic.Presenters
{
    public class DatabasesListPresenter
    {
        private string _networkPath = string.Empty;
        private readonly IFormFactory<IDatabasesListView> _formFactory;
        private readonly INetworkConnection _networkConnection;
        private readonly INetworkPathInfo _networkPathInfo;
        private IDatabasesListView _view;
        private readonly IFileManager _fileManager;
        private readonly IMetaData _metaData;
        private readonly IIoc _ioc;
        private readonly IChecksum _checksum;
        private readonly IResumableFileManager _resumableFileManager;
        private readonly IUserAccessCredentials _userAccessCredentials;

        private readonly DatabaseDetailsPresenter _databaseDetailsPresenter;
        private readonly LoginPresenter _loginPresenter;
        private CancellationTokenSource _cancellationTokenSource;
        public DatabasesListPresenter(IChecksum checksum, IUserAccessCredentials userAccessCredentials, IFileManager fileManager, IMetaData metaData, IIoc ioc,
            DatabaseDetailsPresenter databaseDetailsPresenter, LoginPresenter loginPresenter, IFormFactory<IDatabasesListView> formFactory,
            INetworkConnection networkConnection, INetworkPathInfo networkPathInfo)
        {
            _databaseDetailsPresenter = databaseDetailsPresenter;
            _loginPresenter = loginPresenter;
            _checksum = checksum;
            _userAccessCredentials = userAccessCredentials;
            _fileManager = fileManager;
            _metaData = metaData;
            _ioc = ioc;
            _formFactory = formFactory;
            _networkConnection = networkConnection;
            _networkPathInfo = networkPathInfo;
            _view = _formFactory.CreateDatabasesListForm();
            _resumableFileManager = _fileManager as IResumableFileManager;
            _cancellationTokenSource = new CancellationTokenSource();
            _networkPath = _networkPathInfo.GetPathNeworkDirectory();
        }
        private void BindCommands()
        {
            _view.Upload = new AsyncCommand(UploadAction);
            _view.Find = new BaseCommand(FindCommand);
            _view.PauseCommand = new BaseCommand(new Action(() =>
            {
                _cancellationTokenSource.Cancel();
            }));
            _view.GetInfoDatabaseCommand = new BaseCommand(new Action<object>((id) =>
            {
                try
                {
                    var databaseDetails = _databaseDetailsPresenter;
                    databaseDetails.Init(Convert.ToInt32(id));
                    databaseDetails.LoadInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        public void FindCommand()
        {
            var tagsToCheck = _view.Model.FindByInput;
            var findBySelectedItem = _view.Model.ComboboxSelectedItem;
            var table = _metaData.ReadInfo();
            DataTable tableFoundValue = new DataTable();
            tableFoundValue = table.Clone();
            if (!string.IsNullOrEmpty(tagsToCheck))
            {
                List<string> tagsFromInput = tagsToCheck.Replace(" ", string.Empty).Split(new char[] { ',', ';' }).ToList();
                foreach (DataRow tableRow in table.Rows)
                {
                    var currentRow = tableRow[findBySelectedItem + 1].ToString();

                    var rowToList = currentRow.Replace(" ", string.Empty).Split(new char[] { ',', ';' }).ToList();

                    if (!tagsFromInput.Except(rowToList).Any())
                    {
                        tableFoundValue.ImportRow(tableRow);
                    }
                }
                _view.Model.DataTable = tableFoundValue;
            }
            else
            {
                LoadTable();
            }
        }
        private async Task UploadAction()
        {
            if (_view.CheckTextboxesEmpty()) return;
            try
            {
                _resumableFileManager.ProcessingFinishedDelegate = UploadFinshedDelegateHandler;
                _resumableFileManager.ProgressbarChangedDelegate = UploadProgressChangedHandler;
                _resumableFileManager.StatusChangedDelegate = UpdateStatusChangedHandler;
                _cancellationTokenSource = new CancellationTokenSource();
                var targetFile = string.Empty;
                targetFile = await _resumableFileManager.Upload(_view.Model.PathToFile, _networkPath, string.Empty, false, _cancellationTokenSource.Token);
                if (!string.IsNullOrEmpty(targetFile))
                {
                    var id = _metaData.WriteInfo(_view.Model.DatabaseName, _view.Model.Company, _view.Model.Tags);
                    var checksumForNewFile = _checksum.CalculateChecksum(_view.Model.PathToFile);
                    _metaData.WriteDetails(id, targetFile.ToString(), checksumForNewFile);
                }
                _view.ClearModel();
                LoadTable();
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
                _view.Model.StatusProgressbar = 100;
                MessageBox.Show("Uploading finished successfully");
            }
        }
        private void UploadProgressChangedHandler(long totalBytes, long sizeOfFile)
        {
            var percent = (int)((double)totalBytes / sizeOfFile * 100);
            _view.Model.StatusProgressbar = percent;
        }
        private void UpdateStatusChangedHandler(long totalBytes, long sizeOfFile, long processingSpeed)
        {
            _view.Model.UpdateStatus = $"{totalBytes / 1048576}/{sizeOfFile / 1048576}  MB  {processingSpeed / 1048576} MB/s";
        }
        public void LoadTable()
        {
            _view.Model.DataTable = _metaData.ReadInfo();
        }
        public IDatabasesListView Run()
        {
            try
            {
                var permissions = _userAccessCredentials.ReadCredentials();

                var myForm = _loginPresenter;
                if (permissions == null) myForm.Run();
                if (!myForm.CheckPermissions())
                {
                    _view.CloseDialog();
                    return null;
                }
                _resumableFileManager.DeleteIncomplitedOldFiles();
                _view.Model = new Model.DatabasesListModel(_view.SynchronizationContext);
                LoadTable();
                BindCommands();
                
                return _view;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }
    }
}