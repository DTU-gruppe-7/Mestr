using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mestr.Data.DbContext
{
    internal class SqliteDbContext
    {
        private static SqliteDbContext? _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;
        private SqliteConnection? _connection;
        private SqliteDbContext()
        {
            // Store in user's AppData folder
            // %LocalAppData%\Mestr\data.db
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appDataPath, "Mestr");
            Directory.CreateDirectory(dbFolder); // Ensure folder exists
            var dbPath = Path.Combine(dbFolder, "data.db");
            
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        public static SqliteDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock) // ✅ ADDED thread safety
                    {
                        if (_instance == null)
                        {
                            _instance = new SqliteDbContext();
                        }
                    }
                }
                return _instance;
            }
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            // Create Mestr tables
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Projects (
                    uuid UUID PRIMARY KEY,
                    name TEXT NOT NULL,
                    createdDate DATE NOT NULL,
                    startDate DATE NOT NULL,
                    endDate DATE,
                    description TEXT,
                    status TEXT NOT NULL CHECK (status IN ('Planlagt', 'Aktiv', 'Afsluttet', 'Aflyst'))
                );

                CREATE TABLE IF NOT EXISTS Expenses (
                    uuid UUID PRIMARY KEY,
                    projectUuid UUID NOT NULL,
                    description TEXT NOT NULL,
                    amount DECIMAL(10,2) NOT NULL,
                    date DATE NOT NULL,
                    category TEXT NOT NULL,
                    isAccepted BOOLEAN NOT NULL,
                    FOREIGN KEY (projectUuid) REFERENCES Projects(uuid)
                );

                CREATE TABLE IF NOT EXISTS Earnings (
                    uuid UUID PRIMARY KEY,
                    projectUuid UUID NOT NULL,
                    description TEXT NOT NULL,
                    amount DECIMAL(10,2) NOT NULL,
                    date DATE NOT NULL,
                    isPaid BOOLEAN NOT NULL,
                    FOREIGN KEY (projectUuid) REFERENCES Projects(uuid)
                );
            ";
            command.ExecuteNonQuery();
        }

        public SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                lock (_lock) // ✅ ADDED thread safety
                {
                    if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                    {
                        _connection?.Dispose();
                        _connection = new SqliteConnection(_connectionString);
                        _connection.Open();
                    }
                }
            }
            return _connection;
        }

        public SqliteTransaction BeginTransaction(SqliteConnection connection)
        {
            return connection.BeginTransaction();
        }

        public void CloseConnection()
        {
            lock (_lock) // ✅ ADDED thread safety
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
    }
}
