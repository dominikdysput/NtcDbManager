using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbManager.Logic.Interfaces
{
    public interface IResumableFileManager : IFileManager
    {
        Action<long, long> ProgressbarChangedDelegate { get; set; }
        Action<bool, Exception> ProcessingFinishedDelegate { get; set; }
        Action<long, long, long> StatusChangedDelegate { get; set; }

        Task ResumeDownload(string sourceFilePath, string targetFilePath, string checksum,
            bool resumeDownload,
            CancellationToken? cancellationToken = null);
        Task<string> ResumeUpload(string sourcePath, string targetFilePath, string checksum,
            bool resumeUpload,
            CancellationToken? cancellationToken = null);
        bool CheckInfoFileIsAlreadyDownloaded(string pathToFile, string checksum);
        void SaveFileInfo(string checksum, long totalBytes, string targetPath);
        string[] LoadFileInfo(string targetFile, string checksum);
        bool IsTheSameSize(string sourcePath, string targetFilePath);
        void DeleteIncomplitedOldFiles();
    }
}
