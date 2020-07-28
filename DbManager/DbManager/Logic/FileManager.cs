
using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic
{
    public delegate void ProgressbarChangedDelegate(long bytes, long totalBytes);
    public delegate void ProcessingFinishedDelegate(bool status, Exception exception);
    public delegate void StatusChangedDelegate(long bytes, long totalBytes, long processingSpeed);

    class FileManager : IResumableFileManager
    {
        private long _totalBytes { get; set; } = 0;
        private long _sizeOfFile { get; set; } = 0;
        private long _totalBytesPrev { get; set; } = 0;
        private long _processingSpeed { get; set; } = 0;
        private readonly INetworkConnection _networkConnection;
        private readonly IUserAccessCredentials _userAccessCredentials;
        private readonly INetworkPathInfo _networkPathInfo;
        private readonly IMetaData _metaData;
        private readonly ISqlDatabaseFactory _sqlDatabaseFactory;
        private readonly IChecksum _checksum;

        public Action<long, long> ProgressbarChangedDelegate { get; set; }
        public Action<bool, Exception> ProcessingFinishedDelegate { get; set; }
        public Action<long, long, long> StatusChangedDelegate { get; set; }
        public FileManager(INetworkConnection networkConnection, IUserAccessCredentials userAccessCredentials, INetworkPathInfo networkPathInfo,
            IMetaData metaData, ISqlDatabaseFactory sqlDatabaseFactory, IChecksum checksum)
        {
            _networkConnection = networkConnection;
            _userAccessCredentials = userAccessCredentials;
            _networkPathInfo = networkPathInfo;
            _metaData = metaData;
            _sqlDatabaseFactory = sqlDatabaseFactory;
            _checksum = checksum;
        }
        public async Task Download(string sourcePath, string targetPath, string checksum, bool resumeDownload,
            CancellationToken cancellationToken)
        {
            string tmpFilePath = targetPath + @"\\" + Path.GetFileName("tmpFile.txt");
            string targetFilePath = targetPath + @"\\" + Path.GetFileName(sourcePath);
            DeleteRowFromMetaDataFile(checksum, tmpFilePath);
            try
            {
                if (!resumeDownload)
                {
                    if (!IsTheSameSize(sourcePath, targetFilePath))
                    {
                        Clean(targetFilePath);
                    }
                }
                if (IsTheSameSize(sourcePath, targetFilePath) && IsTheSameFile(checksum, targetFilePath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                    return;
                }
                var path = await FileTransfer(sourcePath, targetFilePath, checksum, tmpFilePath, true, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
            }
        }
        public async Task<string> Upload(string sourcePath, string targetPath, string checksum, bool resumeUpload,
            CancellationToken cancellationToken)
        {
            var tmpFilePath = _networkPathInfo.GetPathMetaDataFile();
            targetPath = GetFileName(sourcePath, checksum);
            DeleteRowFromMetaDataFile(checksum, tmpFilePath);
            try
            {
                if (!IsTheSameSize(sourcePath, targetPath) && !resumeUpload)
                {
                    Clean(targetPath);
                }

                if (IsTheSameSize(sourcePath, targetPath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }
                return await FileTransfer(sourcePath, targetPath, checksum, tmpFilePath, resumeUpload, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
                return string.Empty;
            }
        }
        private async Task<string> FileTransfer(string sourcePath, string targetPath, string checksum, string tmpFilePath, bool resumeProcessing,
            CancellationToken cancellationToken)
        {
            var credentials = _userAccessCredentials.ReadCredentials();
            _totalBytes = 0;
            _sizeOfFile = 0;
            _processingSpeed = 0;
            if (!targetPath.Contains(".tmp"))
                targetPath = $"{targetPath}.tmp";
            using (var connection = _networkConnection.Connect(_networkPathInfo.GetNetworkPath(), credentials.Login, credentials.Password))
            {
                try
                {
                    byte[] buffer = new byte[16384];
                    int readBytes;

                    if (File.Exists(targetPath))
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.Hidden);
                    using (FileStream sourceStream = File.OpenRead(sourcePath))
                    using (FileStream targetStream = File.Create(targetPath))
                    {
                        if (targetStream.Length == sourceStream.Length)
                        {
                            ProcessingFinishedDelegate?.Invoke(true, null);
                            return string.Empty;
                        }
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.Hidden);
                        _sizeOfFile = sourceStream.Length;

                        using (var timer = new System.Threading.Timer(FormStatusUpdate, null, 1000, 1000))
                        {
                            while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                                await targetStream.WriteAsync(buffer, 0, readBytes);
                                _totalBytes += readBytes;
                            }
                        }
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.Hidden);
                        ProcessingFinishedDelegate?.Invoke(true, null);
                    }
                    string newPath = $"{ Path.GetDirectoryName(targetPath)}\\{Path.GetFileNameWithoutExtension(targetPath)}";
                    File.Move(targetPath, newPath);
                    return newPath;
                }
                catch (Exception ex)
                {
                    if (resumeProcessing == true)
                        SaveFileInfo(checksum, _totalBytes, tmpFilePath, Path.GetFileName(targetPath));
                    else
                    {
                        File.Delete(targetPath);
                        DeleteRowFromMetaDataFile(checksum, tmpFilePath);
                    }
                    ProcessingFinishedDelegate?.Invoke(false, ex);
                }
                return string.Empty;
            }
        }
        public void FormStatusUpdate(object timerState)
        {
            ProgressbarChangedDelegate?.Invoke(_totalBytes, _sizeOfFile);
            StatusChangedDelegate?.Invoke(_totalBytes, _sizeOfFile, _processingSpeed);
            _processingSpeed = _totalBytes - _totalBytesPrev;
            if (_processingSpeed < 0) _processingSpeed = 0;
            _totalBytesPrev = _totalBytes;
        }
        private async Task<string> ResumeFileTransfer(string sourcePath, string targetPath, long totalBytes, string tmpFilePath, string checksum,
            CancellationToken cancellationToken)
        {
            var credentials = _userAccessCredentials.ReadCredentials();
            using (var connection = _networkConnection.Connect(_networkPathInfo.GetNetworkPath(), credentials.Login, credentials.Password))
            {
                try
                {
                    _totalBytes = totalBytes;
                    _processingSpeed = 0;
                    _sizeOfFile = 0;
                    byte[] buffer = new byte[16384];
                    int readBytes;
                    using (FileStream sourceStream = File.OpenRead(sourcePath))
                    using (FileStream targetStream = File.OpenWrite(targetPath))
                    {
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.Hidden);
                        if (_totalBytes != 0)
                        {
                            targetStream.Seek(_totalBytes, SeekOrigin.Begin);
                            sourceStream.Seek(_totalBytes, SeekOrigin.Begin);
                        }
                        _sizeOfFile = sourceStream.Length;
                        using (var stateTimer = new System.Threading.Timer(FormStatusUpdate, null, 1000, 1000))

                        {
                            while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    return string.Empty;
                                }
                                await targetStream.WriteAsync(buffer, 0, readBytes);
                                _totalBytes += readBytes;
                            }
                        }
                        DeleteRowFromMetaDataFile(checksum, tmpFilePath);
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.Hidden);
                        ProcessingFinishedDelegate?.Invoke(true, null);
                    }
                    string newPath = $"{ Path.GetDirectoryName(targetPath)}\\{Path.GetFileNameWithoutExtension(targetPath)}";
                    File.Move(targetPath, newPath);
                    return targetPath;
                }
                catch (Exception ex)
                {
                    DeleteRowFromMetaDataFile(checksum, tmpFilePath);
                    SaveFileInfo(checksum, _totalBytes, tmpFilePath, Path.GetFileName(targetPath));
                    ProcessingFinishedDelegate?.Invoke(false, ex);
                }
                return string.Empty;
            }
        }
        public bool CheckInfoFileIsAlreadyDownloaded(string pathToFile, string checksum)
        {
            FileInfo fInfo = new FileInfo(pathToFile);
            if (fInfo.Exists)
            {
                using (StreamReader sourceStream = new StreamReader(pathToFile))
                {
                    string[] data = null;
                    string line = null;
                    while ((line = sourceStream.ReadLine()) != null)
                    {
                        data = line.Split(';').ToArray();
                        if (checksum.Equals(data[0]))
                            return true;
                    }
                }
            }
            return false;
        }
        private long GetTotalBytes(string tmpFilePath, string checksum, long totalBytes)
        {
            var metadataDownloadedFile = LoadFileInfo(tmpFilePath, checksum);
            if (metadataDownloadedFile != null)
            {
                if (!checksum.Equals(metadataDownloadedFile[0]))
                    throw new Exception();
                totalBytes = long.Parse(metadataDownloadedFile[1]);
            }
            return totalBytes;
        }
        private string GetFileName(string pathToFile, string checksum)
        {
            var networkPath = _networkPathInfo.GetPathNeworkDirectory();
            string pathToMetaDataFile = _networkPathInfo.GetPathMetaDataFile();
            string line = string.Empty;
            string[] receivedData = null;
            if (File.Exists(pathToMetaDataFile))
            {
                using (StreamReader sourceStream = new StreamReader(pathToMetaDataFile))
                {
                    while ((line = sourceStream.ReadLine()) != null)
                    {
                        receivedData = line.Split(';').ToArray();
                        if (receivedData[0].Equals(checksum))
                            return networkPath + receivedData[3];
                    }
                }
            }

            var fileName = Path.GetFileName(pathToFile);
            fileName = networkPath + fileName;

            int number = 0;
            while (File.Exists(fileName))
            {
                fileName = GetSubstring(Path.GetFileNameWithoutExtension(pathToFile));
                fileName = networkPath + fileName + "_" + number + Path.GetExtension(pathToFile);
                number++;
            }

            return fileName;
        }
        private string GetSubstring(string stringToSubstring)
        {
            int index = stringToSubstring.IndexOf("_");
            if (index > 0)
                stringToSubstring = stringToSubstring.Substring(0, index);

            return stringToSubstring;
        }
        public void SaveFileInfo(string checksum, long totalBytes, string targetPath, string fileName)
        {
            string dataString = $"{checksum};{totalBytes};{DateTime.Now.ToShortDateString()};{fileName}\n";
            File.AppendAllText(targetPath, dataString);
        }
        public string[] LoadFileInfo(string targetFile, string checksum)
        {
            string[] data = null;
            FileInfo fInfo = new FileInfo(targetFile);
            if (fInfo.Exists)
            {
                try
                {
                    using (StreamReader sourceStream = new StreamReader(targetFile))
                    {
                        string line = null;
                        while ((line = sourceStream.ReadLine()) != null)
                        {
                            data = line.Split(';').ToArray();
                            if (checksum.Equals(data[0]))
                                return data;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return null;
        }
        public void Clean(string targetPath)
        {
            FileInfo targetFile = new FileInfo(targetPath);
            if (targetFile.Exists)
            {
                try
                {
                    targetFile.Delete();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        public bool IsTheSameSize(string sourcePath, string targetPath)
        {
            FileInfo sourceFile = new FileInfo(sourcePath);
            FileInfo targetFile = new FileInfo(targetPath);
            if (sourceFile.Exists && targetFile.Exists)
                if (sourceFile.Length == targetFile.Length)
                    return true;
            return false;
        }
        public async Task ResumeDownload(string sourcePath, string targetPath, string checksum,
            bool resumeDownload, CancellationToken cancellationToken)
        {
            long totalBytes = 0;
            string tmpFilePath = targetPath + @"\\" + Path.GetFileName("tmpFile.txt");
            targetPath = targetPath + @"\\" + Path.GetFileName(sourcePath) + ".tmp";
            try
            {
                totalBytes = GetTotalBytes(tmpFilePath, checksum, totalBytes);

                if (!IsTheSameSize(sourcePath, targetPath) && !resumeDownload)
                {
                    Clean(targetPath);
                }

                if (IsTheSameSize(sourcePath, targetPath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }
                var path = await ResumeFileTransfer(sourcePath, targetPath, totalBytes, tmpFilePath, checksum, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
            }
        }
        public async Task<string> ResumeUpload(string sourcePath, string targetPath, string checksum,
            bool resumeUpload, CancellationToken cancellationToken)
        {
            long totalBytes = 0;
            string tmpFilePath = _networkPathInfo.GetPathMetaDataFile();
            targetPath = targetPath + @"\\" + Path.GetFileName(sourcePath);
            targetPath = $"{targetPath}.tmp";
            try
            {
                totalBytes = GetTotalBytes(tmpFilePath, checksum, totalBytes);
                if (!IsTheSameSize(sourcePath, targetPath) && !resumeUpload)
                {
                    Clean(targetPath);
                }

                if (IsTheSameSize(sourcePath, targetPath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }

                return await ResumeFileTransfer(sourcePath, targetPath, totalBytes, tmpFilePath, checksum, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
                return string.Empty;
            }
        }
        public void DeleteIncomplitedOldFiles()
        {
            try
            {
                int timeOfDeprecated = 10;
                var pathToFolder = _networkPathInfo.GetPathNeworkDirectory();
                var pathToMetaDataFile = _networkPathInfo.GetPathMetaDataFile();
                DirectoryInfo directory = new DirectoryInfo(pathToFolder);
                FileInfo[] files = directory.GetFiles("*.tmp");
                foreach (var file in files)
                {
                    var path = Path.GetFullPath(file.FullName);

                    DateTime startDate = File.GetLastWriteTime(path);
                    DateTime endDate = DateTime.Now;
                    if ((endDate - startDate).TotalDays > timeOfDeprecated)
                        File.Delete(path);
                }
                if (File.Exists(pathToMetaDataFile))
                {
                    var targetFile = _networkPathInfo.GetPathMetaDataFile();
                    string[] receivedData = null;
                    string line;
                    List<string> dataToSend = new List<string>();
                    using (StreamReader sourceStream = new StreamReader(targetFile))
                    {
                        while ((line = sourceStream.ReadLine()) != null)
                        {
                            receivedData = line.Split(';').ToArray();
                            var date = Convert.ToDateTime(receivedData[2]);
                            if ((DateTime.Now - date).TotalDays <= timeOfDeprecated)
                                dataToSend.Add(line + "\n");
                        }
                    }
                    using (StreamWriter targetStream = new StreamWriter(targetFile))
                    {
                        foreach (var lineToSend in dataToSend)
                        {
                            targetStream.Write(lineToSend);
                        }
                    }
                }
                CheckFileAvailability();
                CheckDbDetailsInfoExists();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public void DeleteRowFromMetaDataFile(string checksum, string tmpFilePath)
        {
            try
            {
                var targetFile = tmpFilePath;
                if (File.Exists(targetFile))
                {
                    List<string> dataToSend = new List<string>();
                    using (StreamReader sourceStream = new StreamReader(targetFile))
                    {
                        string[] receivedData = null;
                        string line;

                        while ((line = sourceStream.ReadLine()) != null)
                        {
                            receivedData = line.Split(';').ToArray();
                            if (!receivedData[0].Equals(checksum))
                                dataToSend.Add(line);
                        }
                    }
                    using (StreamWriter targetStream = new StreamWriter(targetFile))
                    {
                        foreach (var lineToSend in dataToSend)
                        {
                            targetStream.WriteLine(lineToSend);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void CheckFileAvailability()
        {
            try
            {
                var dbDetails = _metaData.ReadAllDetails();
                using (var conn = _sqlDatabaseFactory.GetConnection())
                {
                    foreach (DataRow row in dbDetails.Rows)
                    {
                        var pathToFile = row.Field<string>("PathToFile");
                        if (!File.Exists(pathToFile))
                        {
                            var command = conn.CreateCommand();
                            command.CommandText = "DELETE FROM DbDetails WHERE PathToFile=@pathToFile";
                            command.Parameters.AddWithValue("@pathToFile", pathToFile);
                            conn.Open();
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void CheckDbDetailsInfoExists()
        {
            try
            {
                using (var conn = _sqlDatabaseFactory.GetConnection())
                {
                    var command = conn.CreateCommand();
                    command.CommandText = "DELETE FROM DbInfo WHERE NOT EXISTS (SELECT 1 FROM DbDetails WHERE DbDetails.Id = DbInfo.Id )";
                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool IsTheSameFile(string checksumSourceFile, string targetPath)
        {
            if (!File.Exists(targetPath))
                return false;

            var checksumTargetFile = _checksum.CalculateChecksum(targetPath);

            if (checksumSourceFile.Equals(checksumTargetFile))
                return true;

            return false;
        }
    }
}

