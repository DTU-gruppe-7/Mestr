using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using Mestr.Core.Model;
using Mestr.Core.Enum;

namespace Mestr.Data.Repository
{
    public class ExpenseRepository : IRepository<Expense>
    {
        private readonly SqliteDbContext _dbContext;
        private readonly SqliteConnection _connection;

        public ExpenseRepository() {
            _dbContext = SqliteDbContext.Instance;
            _connection = _dbContext.GetConnection();
        }
        public void Add(Expense entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            using var command = _connection.CreateCommand();
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
        }

        public Expense? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM expenses WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid);


            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var expense = new Expense(
                    Guid.Parse(reader["uuid"].ToString()!),
                    Guid.Parse(reader["projectuuid"].ToString()!),
                    reader["description"].ToString()!,
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    Enum.TryParse<ExpenseCategory>(reader["category"].ToString(), out var status)
                    ? status
                    : ExpenseCategory.Other, // default fallback value
                    reader.GetBoolean(reader.GetOrdinal("isAccepted"))
                );
                return expense;
            }

            return null;
        }

        public IEnumerable<Expense> GetAll()
        {
            var expenses = new List<Expense>();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM expenses";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var expense = new Expense(
                    Guid.Parse(reader["uuid"].ToString()!),
                    Guid.Parse(reader["projectuuid"].ToString()!),
                    reader["description"].ToString()!,
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    Enum.TryParse<ExpenseCategory>(reader["category"].ToString(), out var status)
                    ? status
                    : ExpenseCategory.Other, // default fallback value
                    reader.GetBoolean(reader.GetOrdinal("isAccepted"))
                    );
           
                expenses.Add(expense);
            }

            return expenses;
        }

        public void Update(Expense entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE expenses " +
                "SET description = @description, " +
                "amount = @amount, " +
                "date = @date, " +
                "category = @category, " +
                "isAccepted = @isAccepted " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@category", entity.Category.ToString());
            command.Parameters.AddWithValue("@isAccepted", entity.IsAccepted);
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.ExecuteNonQuery();
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM expenses WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid);
            command.ExecuteNonQuery();
        }
    }
}