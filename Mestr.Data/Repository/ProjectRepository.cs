using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices.Marshalling;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        public void Add(Project uuid)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Projects (uuid, name, startDate, endDate, description, status, createdDate)" +
                "VALUES (@uuid, @name, @startDate, @endDate, @description, @status, @createdDate);";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.Parameters.AddWithValue("@name", entity.Name);
            command.Parameters.AddWithValue("@startDate", entity.StartDate);
            command.Parameters.AddWithValue("@endDate", entity.EndDate);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@status", entity.Status);
            command.Parameters.AddWithValue("@createdDate", entity.CreatedDate);
            command.ExecuteNonQuery();
            connection.Close();

            // Udkommenteret, hvis datatypen skal defineres eksplicit
            // command.Parameters.Add("@uuid", System.Data.DbType.Int32).Value = entity.Id;
            // command.Parameters.Add("@Name", System.Data.DbType.String).Value = entity.Name;
            // command.Parameters.Add("@startDate", System.Data.DbType.DateTime).Value = entity.StartDate;
            // command.Parameters.Add("@endDate", System.Data.DbType.DateTime).Value = entity.EndDate;
            // command.Parameters.Add("@description", System.Data.DbType.String).Value = entity.Description;
            // command.Parameters.Add("@status", System.Data.DbType.String).Value = entity.Status;
            // command.Parameters.Add("@createdDate", System.Data.DbType.DateTime).Value = entity.CreatedDate;

        }

        public Project GetByUUID(Guid uuid)
        {
            if (uuid == null)
            {
                throw new ArgumentNullException(nameof(uuid));
            }


            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM projects WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                var project = new Project
                {
                    uuid = reader.GetInt32(0),
                    name = reader.GetString(1),
                    startDate = reader.GetDateTime(2),
                    endDate = reader.GetDateTime(3),
                    description = reader.GetString(4),
                    status = reader.GetString(5),
                    createdDate = reader.GetDateTime(6)
                };
                connection.Close();
                return project;
            }

        }

        public IEnumerable<Project> GetAll()
        {
            var projects = new List<Project>();

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Projects";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var project = new Project
                {
                    uuid = Guid.Parse(reader["uuid"].ToString()),
                    name = reader["name"].ToString(),
                    startDate = reader.GetDateTime(reader.GetOrdinal("startDate")),
                    endDate = reader.GetDateTime(reader.GetOrdinal("endDate")),
                    description = reader["description"].ToString(),
                    status = reader["status"].ToString(),
                    createdDate = reader.GetDateTime(reader.GetOrdinal("createdDate"))
                };
                projects.Add(project);
            }

            connection.Close();
            return projects;
        }

        public void Update(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE projects " +
                "SET name = @name " +
                "SET startDate = @startDate " +
                "SET endDate = @endDate " +
                "SET description = @description " +
                "SET status = @status " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.uuid.ToString());
            command.Parameters.AddWithValue("@name", entity.name);
            command.Parameters.AddWithValue("@startDate", entity.startDate);
            command.Parameters.AddWithValue("@endDate", entity.endDate);
            command.Parameters.AddWithValue("@description", entity.description);
            command.Parameters.AddWithValue("@status", entity.status);
            command.ExecuteNonQuery();
            connection.Close();

        }

        public void Delete(Guid uuid)
        {
            if (uuid == null)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM projects WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}