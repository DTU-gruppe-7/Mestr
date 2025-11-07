using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Mestr.Data.DbContext
{
    internal class SqliteDbContext
    {
        private readonly string _connectionString;
        public SqliteDbContext()
        {
            _connectionString = $"Data Source=data.db";
        }
        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
        public SqliteTransaction BeginTransaction(SqliteConnection connection)
        {
            return connection.BeginTransaction();
        }

    }
}
