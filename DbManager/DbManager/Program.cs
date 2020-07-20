using DbManager.Infrastructure;
using DbManager.Logic;
using DbManager.Logic.Connection;
using DbManager.Logic.Interfaces;
using DbManager.Logic.Interfaces.ViewInterfaces;
using DbManager.Logic.Model;
using DbManager.Logic.Presenters;
using DbManager.View;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var ioc = SetUpContainer();
            var mainForm = ioc.GetInstance<DatabasesListPresenter>();
            var view = mainForm.Run();
            if (view != null)
                Application.Run((Form)view);
        }

        private static IIoc SetUpContainer()
        {
            var ioc = new Container();
            ioc.Options.EnableAutoVerification = false;

            ioc.Register<IDatabasesListView, DatabasesList>();
            ioc.Register<ILoginView, Login>();
            ioc.Register<IDownloadSelectedVersionView, DownloadSelectedVersion>();
            ioc.Register<IUploadNewVersionView, UploadNewVersion>();
            ioc.Register<IDatabaseDetailsView, DatabaseDetails>();

            ioc.Register<IMessageService, WindowsMessagesService>();

            ioc.Register<LoginPresenter>();
            ioc.Register<UploadNewVersionPresenter>();
            ioc.Register<DownloadSelectedVersionPresenter>();
            ioc.Register<DatabasesListPresenter>();
            ioc.Register<DatabaseDetailsPresenter>();

            ioc.Register<IFormFactory<ILoginView>, FormFactory<ILoginView>>();
            ioc.Register<IFormFactory<IDatabasesListView>, FormFactory<IDatabasesListView>>();
            ioc.Register<IFormFactory<IDownloadSelectedVersionView>, FormFactory<IDownloadSelectedVersionView>>();
            ioc.Register<IFormFactory<IUploadNewVersionView>, FormFactory<IUploadNewVersionView>>();
            ioc.Register<IFormFactory<IDatabaseDetailsView>, FormFactory<IDatabaseDetailsView>>();

            ioc.Register<IChecksum, ChecksumMD5>();
            ioc.Register<IFileManager, FileManager>();
            ioc.Register<IFileValidator, FileValidator>();
            ioc.Register<IMetaData, SqlMetaData>();
            ioc.Register<IUserAccessCredentials, UserAccessCredentials>();
            ioc.Register<ISqlDatabaseFactory, SqlDatabaseFactory>();

            ioc.Register<INetworkPathInfo, NetworkPathInfo>();
            ioc.Register<INetworkConnection, NetworkConnection>();

            var iocWrapper = new SimpleInjectorIoc(ioc);
            ioc.RegisterInstance<IIoc>(iocWrapper);

            return iocWrapper;
        }

    }

}
