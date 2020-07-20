using DbManager.Logic.Connection;
using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic
{
    class SqlMetaData : IMetaData
    {
        private ISqlDatabaseFactory _sqlDatabaseFactory;

        public SqlMetaData(ISqlDatabaseFactory sqlDatabaseFactory)
        {
            _sqlDatabaseFactory = sqlDatabaseFactory;
        }
        public DataTable ReadDetails(int id)
        {
            DataTable table = new DataTable();
            using (var conn = _sqlDatabaseFactory.GetConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "Select * from DbDetails WHERE Id=@id";
                command.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter();
                using (da = new SqlDataAdapter(command))
                {
                    da.Fill(table);
                }
            }
            return table;
        }
        public DataTable ReadInfo()
        {
            DataTable table = new DataTable();
            using (var conn = _sqlDatabaseFactory.GetConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "Select * from DbInfo";
                SqlDataAdapter da = new SqlDataAdapter();
                using (da = new SqlDataAdapter(command))
                {
                    da.Fill(table);
                }
            }
            return table;
        }
        public DataTable GetInfoById(int id)
        {
            DataTable table = new DataTable();
            using (var conn = _sqlDatabaseFactory.GetConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "Select * from DbInfo WHERE Id=@id";
                command.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter();
                using (da = new SqlDataAdapter(command))
                {
                    da.Fill(table);
                }
            }
            return table;
        }
        public int WriteInfo(string dbName, string company, string tags)
        {
            using (var conn = _sqlDatabaseFactory.GetConnection())
            {
                var command = conn.CreateCommand();
                command.CommandText = "INSERT INTO DbInfo (CompanyName, DatabaseName, Tags) OUTPUT INSERTED.Id VALUES(@companyName,@databaseName,@tags)";
                command.Parameters.AddWithValue("@companyName", company);
                command.Parameters.AddWithValue("@databaseName", dbName);
                command.Parameters.AddWithValue("@tags", tags);
                conn.Open();
                var ret = command.ExecuteScalar() as int?;

                if (!ret.HasValue)
                    throw new InvalidOperationException("Insert return no id");

                return ret.Value;
            }
        }
        public void WriteDetails(int id, string targetFile, string checksumForNewFile)
        {
            using (var conn = _sqlDatabaseFactory.GetConnection())
            {
                var checksum = checksumForNewFile;

                var command = conn.CreateCommand();
                var date = DateTime.Now;
                var upladerName = Environment.UserName;
                var pathToFile = targetFile;

                command.CommandText = "INSERT INTO DbDetails (Id, UploadDate, UploaderName, PathToFile, Checksum) VALUES(@id ,@date,@uploaderName, @pathToFile, @checksum)";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@uploaderName", upladerName);
                command.Parameters.AddWithValue("@pathToFile", pathToFile);
                command.Parameters.AddWithValue("@checksum", checksum);

                conn.Open();
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                conn.Close();
            }
        }
    }
}
