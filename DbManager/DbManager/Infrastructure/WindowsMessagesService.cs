using DbManager.Logic;
using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbManager.Infrastructure
{
    public class WindowsMessagesService : IMessageService
    {
        public bool CheckUserWantsToResumeDownload()
        {
            DialogResult dialogResult = MessageBox.Show("Would you like to resume download?", "Resume", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        public bool CheckUserWantsToResumeUpload()
        {
            DialogResult dialogResult = MessageBox.Show("Would you like to resume upload?", "Resume", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                return true;
            else
                return false;
        }
        public string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database files |*.db;*.bak;*.zip;*.rar;*.7zip|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }
            return openFileDialog.FileName;
        }
        public string ShowOpenFolderDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return string.Empty;
            }
            return folderBrowserDialog.SelectedPath;
        }
    }
}
