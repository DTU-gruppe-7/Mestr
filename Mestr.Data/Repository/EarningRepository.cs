using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices.Marshalling;
using Mestr.Core.Model;
using Mestr.Core.Enum;

namespace Mestr.Data.Repository
{
    public class EarningRepository : IRepository<Earning>
    {
        private readonly SqliteDbContext _dbContext;
        private readonly SqliteConnection _connection;
        public EarningRepository() {
            _dbContext = SqliteDbContext.Instance;
            _connection = _dbContext.GetConnection();
        }
        public void Add(Earning entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO earnings (uuid, projectuuid, description, amount, date, isPaid) " +
                "VALUES (@uuid, @projectUuid, @description, @amount, @date, @isPaid);";
            //Get the value from entity uuid property   
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@projectUuid", entity.ProjectUuid);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@isPaid", entity.IsPaid);
            command.ExecuteNonQuery();
        }

        public Earning? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM earnings WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid);  

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning(
                    Guid.Parse(reader["uuid"].ToString()!),
                    Guid.Parse(reader["projectuuid"].ToString()!),
                    reader["description"].ToString()!,
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    reader.GetBoolean(reader.GetOrdinal("isPaid"))
                    );
                return earning;
            }

            return null;
        }

        public IEnumerable<Earning> GetAll()
        {
            var earnings = new List<Earning>();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM earnings";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning(
                    Guid.Parse(reader["uuid"].ToString()!),
                    Guid.Parse(reader["projectuuid"].ToString()!),
                    reader["description"].ToString()!,
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    reader.GetBoolean(reader.GetOrdinal("isPaid"))
                );
                earnings.Add(earning);
            }

            return earnings;
        }

        public void Update(Earning entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE earnings " +
                "SET description = @description, " +
                "amount = @amount, " +
                "date = @date, " +
                "isPaid = @isPaid " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@isPaid", entity.IsPaid);
            command.ExecuteNonQuery();
        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM earnings WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid);
            command.ExecuteNonQuery();
        }
    }
}