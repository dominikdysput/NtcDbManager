
using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
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
        private long _totalBytes { get; set; }
        private long _sizeOfFile { get; set; }
        private long _totalBytesPrev { get; set; }
        private long _processingSpeed { get; set; }
        private readonly INetworkConnection _networkConnection;
        private readonly IUserAccessCredentials _userAccessCredentials;
        private readonly INetworkPathInfo _networkPathInfo;
        public Action<long, long> ProgressbarChangedDelegate { get; set; }
        public Action<bool, Exception> ProcessingFinishedDelegate { get; set; }
        public Action<long, long, long> StatusChangedDelegate { get; set; }

        public FileManager(INetworkConnection networkConnection, IUserAccessCredentials userAccessCredentials, INetworkPathInfo networkPathInfo)
        {
            _networkConnection = networkConnection;
            _userAccessCredentials = userAccessCredentials;
            _networkPathInfo = networkPathInfo;
        }
        public async Task Download(string sourcePath, string targetPath, string checksum, bool resumeDownload,
            CancellationToken? cancellationToken = null)
        {
            string tmpFilePath = targetPath + @"\\" + Path.GetFileName("tmpFile.txt");
            string targetFilePath = targetPath + @"\\" + Path.GetFileName(sourcePath);
            try
            {
                if (!resumeDownload)
                {
                    if (!IsTheSameSize(sourcePath, targetFilePath))
                    {
                        Clean(tmpFilePath, targetFilePath);
                    }
                }
                if (IsTheSameSize(sourcePath, targetFilePath))
                {

                    ProcessingFinishedDelegate?.Invoke(true, null);
                }
                var path = await FileTransfer(sourcePath, targetFilePath, checksum, tmpFilePath, targetFilePath, true, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
            }
        }

        public async Task<string> Upload(string sourcePath, string targetFilePath, string checksum, bool resumeUpload,
            CancellationToken? cancellationToken = null)
        {

            var tmpFilePath = Properties.Settings.Default.NetworkPath + @"\DD_Test\" + Path.GetFileName("tmpFile.txt");
            targetFilePath = GetFileName(sourcePath);
            string targetPath = GetFileName(sourcePath);
            try
            {
                if (!IsTheSameSize(sourcePath, targetFilePath) || !resumeUpload)
                {
                    Clean(tmpFilePath, targetFilePath);
                }

                if (IsTheSameSize(sourcePath, targetFilePath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }
                return await FileTransfer(sourcePath, targetFilePath, checksum, tmpFilePath, targetFilePath, resumeUpload, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
                return string.Empty;
            }
        }
        private async Task<string> FileTransfer(string sourcePath, string targetPath, string checksum, string tmpFilePath, string targetFilePath, bool resumeProcessing,
            CancellationToken? cancellationToken = null)
        {
            var credentials = _userAccessCredentials.ReadCredentials();
            _totalBytes = 0;
            _sizeOfFile = 0;
            _processingSpeed = 0;
            using (var connection = _networkConnection.Connect(_networkPathInfo.GetNetworkPath(), credentials.Login, credentials.Password))
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int readBytes;
                    using (FileStream sourceStream = File.OpenRead(sourcePath))
                    using (FileStream targetStream = File.Create(targetPath))
                    {
                        if (targetStream.Length == sourceStream.Length)
                        {
                            ProcessingFinishedDelegate?.Invoke(true, null);
                            return string.Empty;
                        }
                        if (!cancellationToken.HasValue)
                        {
                            cancellationToken.Value.ThrowIfCancellationRequested();
                        }

                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.Hidden);
                        _sizeOfFile = sourceStream.Length;

                        using (new System.Threading.Timer(FormStatusUpdate, null, 2000, 1000))
                        {
                            while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                if (cancellationToken.Value.IsCancellationRequested)
                                {
                                    cancellationToken.Value.ThrowIfCancellationRequested();
                                }
                                await targetStream.WriteAsync(buffer, 0, readBytes);
                                _totalBytes += readBytes;
                            }
                        }
                        File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.Hidden);
                        ProcessingFinishedDelegate?.Invoke(true, null);
                        return targetPath;
                    }
                }
                catch (Exception ex)
                {
                    if (resumeProcessing == true)
                        SaveFileInfo(checksum, _totalBytes, tmpFilePath);
                    else
                    {
                        File.Delete(targetFilePath);
                        File.Delete(tmpFilePath);
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
            _totalBytesPrev = _totalBytes;
        }
        private async Task<string> ResumeFileTransfer(string sourcePath, string targetFilePath, long totalBytes, string tmpFilePath, string checksum,
            CancellationToken? cancellationToken = null
            )
        {
            var credentials = _userAccessCredentials.ReadCredentials();
            using (var connection = _networkConnection.Connect(_networkPathInfo.GetNetworkPath(), credentials.Login, credentials.Password))
            {
                try
                {
                    _totalBytes = totalBytes;
                    _processingSpeed = 0;
                    _sizeOfFile = 0;
                    byte[] buffer = new byte[8192];
                    int readBytes;
                    using (FileStream sourceStream = File.OpenRead(sourcePath))
                    using (FileStream targetStream = File.OpenWrite(targetFilePath))
                    {
                        File.SetAttributes(targetFilePath, File.GetAttributes(targetFilePath) | FileAttributes.Hidden);
                        if (_totalBytes != 0)
                        {
                            targetStream.Seek(_totalBytes, SeekOrigin.Begin);
                            sourceStream.Seek(_totalBytes, SeekOrigin.Begin);
                        }
                        _sizeOfFile = sourceStream.Length;
                        var stateTimer = new System.Threading.Timer(FormStatusUpdate, null, 1000, 1000);

                        while ((readBytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            if (targetStream.Length == sourceStream.Length)
                            {
                                ProcessingFinishedDelegate?.Invoke(true, null);
                                break;
                            }
                            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                            {
                                cancellationToken.Value.ThrowIfCancellationRequested();
                                return string.Empty;
                            }
                            await targetStream.WriteAsync(buffer, 0, readBytes);
                            _totalBytes += readBytes;
                        }
                        File.Delete(tmpFilePath);
                        File.SetAttributes(targetFilePath, File.GetAttributes(targetFilePath) & ~FileAttributes.Hidden);
                        ProcessingFinishedDelegate?.Invoke(true, null);
                        return targetFilePath;
                    }
                }
                catch (Exception ex)
                {
                    SaveFileInfo(checksum, _totalBytes, tmpFilePath);
                    ProcessingFinishedDelegate?.Invoke(false, ex);
                }
                return string.Empty;
            }
        }
        public bool CheckInfoFileIsAlreadyDownloaded(string pathToFile)
        {
            FileInfo fInfo = new FileInfo(pathToFile + @"\\" + Path.GetFileName("tmpFile.txt"));
            if (fInfo.Exists)
            {
                return true;
            }
            return false;
        }
        private long GetTotalBytes(string tmpFilePath, string checksum, long totalBytes)
        {
            var metadataDownloadedFile = LoadFileInfo(tmpFilePath);
            if (metadataDownloadedFile != null)
            {
                if (!checksum.Equals(metadataDownloadedFile[0]))
                    throw new Exception();
                totalBytes = long.Parse(metadataDownloadedFile[1]);
            }

            return totalBytes;
        }
        private string GetFileName(string pathToFile)
        {
            var fileName = Path.GetFileName(pathToFile);
            string NetworkPath = Properties.Settings.Default.NetworkPath + @"\DD_Test\";

            fileName = NetworkPath + fileName;

            int number = 0;
            while (File.Exists(fileName))
            {
                fileName = GetSubstring(Path.GetFileNameWithoutExtension(pathToFile));
                fileName = NetworkPath + fileName + "_" + number + Path.GetExtension(pathToFile);
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
        public void SaveFileInfo(string checksum, long totalBytes, string targetPath)
        {
            string dataString = checksum + ";" + totalBytes;
            if (File.Exists(targetPath))
                File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.Hidden);
            File.WriteAllText(targetPath, dataString);
            File.SetAttributes(targetPath, File.GetAttributes(targetPath) | FileAttributes.Hidden);
        }
        public string[] LoadFileInfo(string targetFile)
        {
            string[] data = null;
            FileInfo fInfo = new FileInfo(targetFile);
            if (fInfo.Exists)
            {
                try
                {
                    using (StreamReader sourceStream = new StreamReader(targetFile))
                    {
                        var line = sourceStream.ReadLine();
                        data = line.Split(';').ToArray();
                        sourceStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return data;
        }
        public void Clean(string tmpFilePath, string targetFilePath)
        {
            FileInfo tmpFile = new FileInfo(tmpFilePath);
            FileInfo targetFile = new FileInfo(targetFilePath);

            if (tmpFile.Exists)
            {
                try
                {
                    tmpFile.Delete();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
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
        public bool IsTheSameSize(string sourcePath, string targetFilePath)
        {
            FileInfo sourceFile = new FileInfo(sourcePath);
            FileInfo targetFile = new FileInfo(targetFilePath);
            if (sourceFile.Exists && targetFile.Exists)
                if (sourceFile.Length == targetFile.Length)
                    return true;
            return false;
        }
        public async Task ResumeDownload(string sourceFilePath, string targetFilePath, string checksum,
            bool resumeDownload, CancellationToken? cancellationToken = null)
        {
            long totalBytes = 0;
            string tmpFilePath = targetFilePath + @"\\" + Path.GetFileName("tmpFile.txt");
            targetFilePath = targetFilePath + @"\\" + Path.GetFileName(sourceFilePath);
            try
            {
                totalBytes = GetTotalBytes(tmpFilePath, checksum, totalBytes);

                if (!IsTheSameSize(sourceFilePath, targetFilePath) && !resumeDownload)
                {
                    Clean(tmpFilePath, targetFilePath);
                }

                if (IsTheSameSize(sourceFilePath, targetFilePath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }
                var path = await ResumeFileTransfer(sourceFilePath, targetFilePath, totalBytes, tmpFilePath, checksum, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
            }
        }

        public async Task<string> ResumeUpload(string sourcePath, string targetFilePath, string checksum,
            bool resumeUpload, CancellationToken? cancellationToken = null)
        {
            long totalBytes = 0;
            string tmpFilePath = targetFilePath + @"\\" + Path.GetFileName("tmpFile.txt");
            targetFilePath = targetFilePath + @"\\" + Path.GetFileName(sourcePath);

            try
            {
                totalBytes = GetTotalBytes(tmpFilePath, checksum, totalBytes);
                if (!IsTheSameSize(sourcePath, targetFilePath) && !resumeUpload)
                {
                    Clean(tmpFilePath, targetFilePath);
                }

                if (IsTheSameSize(sourcePath, targetFilePath))
                {
                    ProcessingFinishedDelegate?.Invoke(true, null);
                }

                return await ResumeFileTransfer(sourcePath, targetFilePath, totalBytes, tmpFilePath, checksum, cancellationToken);
            }
            catch (Exception ex)
            {
                ProcessingFinishedDelegate?.Invoke(false, ex);
                return string.Empty;
            }
        }
        public void DeleteIncomplitedOldFiles()
        {
            var pathToFolder = _networkPathInfo.GetNetworkPath() + @"/DD_Test";
            DirectoryInfo directory = new DirectoryInfo(pathToFolder);
            FileInfo[] files = directory.GetFiles("*.db");
            foreach (var file in files)
            {
                var path = Path.GetFullPath(file.FullName);
                if (File.GetAttributes(path).HasFlag(FileAttributes.Hidden))
                {
                    DateTime startDate = File.GetLastWriteTime(path);
                    DateTime endDate = DateTime.Now;
                    if((endDate - startDate).TotalDays > 7)
                        File.Delete(path);
                }
            }
        }
    }
}

