using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Logic
{
    public interface IMessageService
    {
        bool CheckUserWantsToResumeDownload();
        bool CheckUserWantsToResumeUpload();
        string ShowOpenFolderDialog();
        string ShowOpenFileDialog();
    }
}
