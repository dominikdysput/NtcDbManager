using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic
{
    public interface IMetaData
    {
        int WriteInfo(string dbName, string company, string tags);
        void WriteDetails(int id, string targetFile, string checksumForNewFile);
        DataTable ReadInfo();
        DataTable GetInfoById(int id);
        DataTable ReadDetails(int id);
        DataTable ReadAllDetails();
    }
}
