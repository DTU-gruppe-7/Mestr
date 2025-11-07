using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices.Marshalling;

namespace Mestr.Data.Repository
{
    public class ExpenseRepository : IRepository<Expense>
    {
        public void Add(Expense uuid)
        {
            if (uuid == null)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO expenses (uuid, projectuuid, description, amount, date, category, receiptpath, isAccepted, project) " +
                "VALUES (@uuid, @projectUuid, @description, @amount, @date, @category, @receiptPath, @isAccepted, @project);";
            command.Parameters.AddWithValue("@uuid", entity.uuid.ToString());
            command.Parameters.AddWithValue("@projectUuid", entity.projectuuid.ToString());
            command.Parameters.AddWithValue("@description", entity.description);
            command.Parameters.AddWithValue("@amount", entity.amount);
            command.Parameters.AddWithValue("@date", entity.date);
            command.Parameters.AddWithValue("@category", entity.category);
            command.Parameters.AddWithValue("@receiptPath", entity.receiptpath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@isAccepted", entity.isAccepted);
            command.Parameters.AddWithValue("@project", entity.project);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public Expense GetByUUID(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM expenses WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var expense = new Expense
                {
                    uuid = Guid.Parse(reader["uuid"].ToString()),
                    projectuuid = Guid.Parse(reader["projectuuid"].ToString()),
                    description = reader["description"].ToString(),
                    amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                    date = reader.GetDateTime(reader.GetOrdinal("date")),
                    category = reader["category"].ToString(),
                    receiptpath = reader["receiptpath"] != DBNull.Value ? reader["receiptpath"].ToString() : null,
                    isAccepted = reader.GetBoolean(reader.GetOrdinal("isAccepted")),
                    project = reader["project"].ToString()
                };
                connection.Close();
                return expense;
            }

            connection.Close();
            return null;
        }

        public IEnumerable<Expense> GetAll()
        {
            var expenses = new List<Expense>();

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM expense";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var expense = new Expense
                {
                    uuid = Guid.Parse(reader["uuid"].ToString()),
                    projectuuid = Guid.Parse(reader["projectuuid"].ToString()),
                    description = reader["description"].ToString(),
                    amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                    date = reader.GetDateTime(reader.GetOrdinal("date")),
                    category = reader["category"].ToString(),
                    receiptpath = reader["receiptpath"] != DBNull.Value ? reader["receiptpath"].ToString() : null,
                    isAccepted = reader.GetBoolean(reader.GetOrdinal("isAccepted")),
                    project = reader["project"].ToString()
                };
                expenses.Add(expense);
            }

            connection.Close();
            return expenses;
        }

        public void Update(Expense entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE expenses " +
                "SET projectuuid = @projectUuid, " +
                "description = @description, " +
                "amount = @amount, " +
                "date = @date, " +
                "category = @category, " +
                "receiptpath = @receiptPath, " +
                "isAccepted = @isAccepted, " +
                "project = @project " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.uuid.ToString());
            command.Parameters.AddWithValue("@projectUuid", entity.projectuuid.ToString());
            command.Parameters.AddWithValue("@description", entity.description);
            command.Parameters.AddWithValue("@amount", entity.amount);
            command.Parameters.AddWithValue("@date", entity.date);
            command.Parameters.AddWithValue("@category", entity.category);
            command.Parameters.AddWithValue("@receiptPath", entity.receiptpath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@isAccepted", entity.isAccepted);
            command.Parameters.AddWithValue("@project", entity.project);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM expenses WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}