using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic.Connection
{
    public class SqlDatabaseFactory : ISqlDatabaseFactory
    {
        public SqlConnection GetConnection()
        {
            return new SqlConnection(Properties.Settings.Default.ConnectionString);
        }
    }
}
