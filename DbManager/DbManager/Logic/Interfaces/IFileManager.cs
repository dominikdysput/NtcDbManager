using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic
{
    public interface IFileManager
    {
        Task<string> Upload(string sourcePath, string targetFilePath, string checksum, bool resumeUpload,
            CancellationToken? cancellationToken = null);
        Task Download(string sourcePath, string targetPath, string checksum, bool resumeDownload,
            CancellationToken? cancellationToken = null);
        void Clean(string targetFilePath);
    }
}
