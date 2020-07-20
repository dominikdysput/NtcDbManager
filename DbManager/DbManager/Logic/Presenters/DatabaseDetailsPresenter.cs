using DbManager.Infrastructure;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic.Presenters
{

    public class DatabaseDetailsPresenter
    {
        private int _id;
        private readonly IFormFactory<IDatabaseDetailsView> _formFactory;
        private readonly IMetaData _sqlMetaDataInfo;
        private IDatabaseDetailsView _view;
        private readonly IMetaData _sqlMetaDataDetails;
        private readonly IIoc _ioc;
        private readonly DownloadSelectedVersionPresenter _downloadSelectedVersionPresenter;
        private readonly UploadNewVersionPresenter _uploadNewVersionPresenter;
        private readonly IUserAccessCredentials _userAccessCredentials;
        private readonly INetworkPathInfo _networkPathInfo;
        private readonly INetworkConnection _networkConnection;

        public DatabaseDetailsPresenter(IMetaData sqlMetaDataInfo, IMetaData sqlMetaDataDetails, IIoc ioc, IFormFactory<IDatabaseDetailsView> formFactory,
            DownloadSelectedVersionPresenter downloadSelectedVersionPresenter, UploadNewVersionPresenter uploadNewVersionPresenter,
            IUserAccessCredentials userAccessCredentials, INetworkPathInfo networkPathInfo, INetworkConnection networkConnection)
        {
            _sqlMetaDataDetails = sqlMetaDataDetails;
            _formFactory = formFactory;
            _downloadSelectedVersionPresenter = downloadSelectedVersionPresenter;
            _uploadNewVersionPresenter = uploadNewVersionPresenter;
            _userAccessCredentials = userAccessCredentials;
            _networkPathInfo = networkPathInfo;
            _networkConnection = networkConnection;
            _sqlMetaDataInfo = sqlMetaDataInfo;
            _ioc = ioc;
        }
        public void Init(int id)
        {
            _id = id;
        }
        private void BindCommands()
        {
            _view.Upload = new BaseCommand(new Action(() =>
            {
                try
                {
                    var myForm = _uploadNewVersionPresenter;
                    myForm.Init(_id);
                    myForm.Run();
                    LoadTable();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
            _view.Download = new BaseCommand(new Action(() =>
            {
                try
                {
                    var cred = _userAccessCredentials.ReadCredentials();
                    if (cred == null)
                        throw new Exception("Cannot to upload file");
                    _networkConnection.TryConnect(_networkPathInfo.GetNetworkPath(), cred.Login, cred.Password);

                    var pathToSource = _view.Model.PathToSource;
                    var checksum = _view.Model.Checksum;
                    var myForm = _downloadSelectedVersionPresenter;
                    myForm.Init(pathToSource, checksum);
                    myForm.Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        public void LoadInfo()
        {
            using (_view = _formFactory.GetForm())
            {
                BindCommands();
                var dbInfoTable = _sqlMetaDataInfo.GetInfoById(_id);
                _view.Model = new Model.DatabaseDetailsModel();
                _view.Model.Company = dbInfoTable.Rows[0][1].ToString();
                _view.Model.DatabaseName = dbInfoTable.Rows[0][2].ToString();
                _view.Model.Tags = dbInfoTable.Rows[0][3].ToString();
                LoadTable();
                _view.ShowDialog();
            }
        }
        public void LoadTable()
        {
            var dbDetailsTable = _sqlMetaDataDetails.ReadDetails(_id);
            _view.Model.DataTable = dbDetailsTable;
        }
    }
}
