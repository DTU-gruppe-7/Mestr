using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices.Marshalling;

namespace Mestr.Data.Repository
{
    public class EarningRepository : IRepository<Earning>
    {
        public void Add(Earning entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
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
            connection.Close();
        }

        public Earning GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM earnings WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning(
                    Guid.Parse(reader["uuid"].ToString()),
                    Guid.Parse(reader["projectuuid"].ToString()),
                    reader["description"].ToString(),
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    reader.GetBoolean(reader.GetOrdinal("isPaid"))
                    );

                connection.Close();
                return earning;
            }

            connection.Close();
            return null;
        }

        // Fix for CS7036: Use the constructor that requires all parameters for Earning in GetAll()
        public IEnumerable<Earning> GetAll()
        {
            var earnings = new List<Earning>();

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM earnings";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning(
                    Guid.Parse(reader["uuid"].ToString()),
                    Guid.Parse(reader["projectuuid"].ToString()),
                    reader["description"].ToString(),
                    reader.GetDecimal(reader.GetOrdinal("amount")),
                    reader.GetDateTime(reader.GetOrdinal("date")),
                    reader.GetBoolean(reader.GetOrdinal("isPaid"))
                );
                earnings.Add(earning);
            }

            connection.Close();
            return earnings;
        }

        public void Update(Earning entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE earnings " +
                "SET projectuuid = @projectUuid, " +
                "description = @description, " +
                "amount = @amount, " +
                "date = @date, " +
                "isPaid = @isPaid, " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@projectUuid", entity.ProjectUuid);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@amount", entity.Amount);
            command.Parameters.AddWithValue("@date", entity.Date);
            command.Parameters.AddWithValue("@isPaid", entity.IsPaid);
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
            command.CommandText = "DELETE FROM earnings WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}