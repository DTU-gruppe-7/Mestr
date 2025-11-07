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
            command.CommandText = "INSERT INTO earning (uuid, projectuuid, description, amount, date, isPaid, paymentDate, method, project) " +
                "VALUES (@uuid, @projectUuid, @description, @amount, @date, @isPaid, @paymentDate, @method, @project);";
            command.Parameters.AddWithValue("@uuid", entity.uuid.ToString());
            command.Parameters.AddWithValue("@projectUuid", entity.projectuuid.ToString());
            command.Parameters.AddWithValue("@description", entity.description);
            command.Parameters.AddWithValue("@amount", entity.amount);
            command.Parameters.AddWithValue("@date", entity.date);
            command.Parameters.AddWithValue("@isPaid", entity.isPaid);
            command.Parameters.AddWithValue("@paymentDate", entity.paymentDate);
            command.Parameters.AddWithValue("@method", entity.method);
            command.Parameters.AddWithValue("@project", entity.project);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public Earning GetByUUID(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM earning WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning
                {
                    uuid = Guid.Parse(reader["uuid"].ToString()),
                    projectuuid = Guid.Parse(reader["projectuuid"].ToString()),
                    description = reader["description"].ToString(),
                    amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                    date = reader.GetDateTime(reader.GetOrdinal("date")),
                    isPaid = reader.GetBoolean(reader.GetOrdinal("isPaid")),
                    paymentDate = reader["paymentDate"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("paymentDate")) : (DateTime?)null,
                    method = reader["method"].ToString(),
                    project = reader["project"].ToString()
                };
                connection.Close();
                return earning;
            }

            connection.Close();
            return null;
        }

        public IEnumerable<Earning> GetAll()
        {
            var earnings = new List<Earning>();

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM earning";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var earning = new Earning
                {
                    uuid = Guid.Parse(reader["uuid"].ToString()),
                    projectuuid = Guid.Parse(reader["projectuuid"].ToString()),
                    description = reader["description"].ToString(),
                    amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                    date = reader.GetDateTime(reader.GetOrdinal("date")),
                    isPaid = reader.GetBoolean(reader.GetOrdinal("isPaid")),
                    paymentDate = reader["paymentDate"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("paymentDate")) : (DateTime?)null,
                    method = reader["method"].ToString(),
                    project = reader["project"].ToString()
                };
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
            command.CommandText = "UPDATE earning " +
                "SET projectuuid = @projectUuid, " +
                "description = @description, " +
                "amount = @amount, " +
                "date = @date, " +
                "isPaid = @isPaid, " +
                "paymentDate = @paymentDate, " +
                "method = @method, " +
                "project = @project " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.uuid.ToString());
            command.Parameters.AddWithValue("@projectUuid", entity.projectuuid.ToString());
            command.Parameters.AddWithValue("@description", entity.description);
            command.Parameters.AddWithValue("@amount", entity.amount);
            command.Parameters.AddWithValue("@date", entity.date);
            command.Parameters.AddWithValue("@isPaid", entity.isPaid);
            command.Parameters.AddWithValue("@paymentDate", entity.paymentDate.HasValue ? (object)entity.paymentDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@method", entity.method);
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
            command.CommandText = "DELETE FROM earning WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}