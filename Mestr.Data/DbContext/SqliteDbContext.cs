using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

namespace Mestr.Data.DbContext
{
    internal class SqliteDbContext
    {
        private readonly string _connectionString;
        public SqliteDbContext()
        {
            // Store in user's AppData folder
            // %appdata%\Local\Mestr\data.db
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appDataPath, "Mestr");
            Directory.CreateDirectory(dbFolder); // Ensure folder exists
            var dbPath = Path.Combine(dbFolder, "data.db");
            
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
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
                    status TEXT NOT NULL CHECK (status IN ('Planned', 'Ongoing', 'Completed', 'Cancelled'))
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
