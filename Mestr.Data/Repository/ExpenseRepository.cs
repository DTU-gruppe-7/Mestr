using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using System;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mestr.Data.Repository
{
    public class ExpenseRepository : IRepository<Expense>
    {
        public void Add(Expense entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO expenses (uuid, projectUuid, description, amount, date, category, isAccepted) " +
                "VALUES (@uuid, @projectUuid, @description, @amount, @date, @category, @isAccepted);";
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@projectUuid", entity.ProjectUuid);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@category", entity.Category);
            command.Parameters.AddWithValue("@isAccepted", entity.IsAccepted);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public Expense GetByUuid(Guid uuid)
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
                var expense = new Expense(
                    Guid.Parse(reader["uuid"].ToString()),
                    Guid.Parse(reader["projectuuid"].ToString()),
                    reader["description"].ToString(),
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    Enum.TryParse<ExpenseCategory>(reader["category"].ToString(), out var status)
                    ? status
                    : ExpenseCategory.Other, // default fallback value
                    reader.GetBoolean(reader.GetOrdinal("isAccepted"))
                );
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
                var expense = new Expense(
                    Guid.Parse(reader["uuid"].ToString()),
                    Guid.Parse(reader["projectuuid"].ToString()),
                    reader["description"].ToString(),
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    Enum.TryParse<ExpenseCategory>(reader["category"].ToString(), out var status)
                    ? status
                    : ExpenseCategory.Other, // default fallback value
                    reader.GetBoolean(reader.GetOrdinal("isAccepted"))
                    );
           
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
                "isAccepted = @isAccepted, " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@projectUuid", entity.ProjectUuid);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@category", entity.Category);
            command.Parameters.AddWithValue("@isAccepted", entity.IsAccepted);
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
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