using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using Mestr.Core.Model;
using Mestr.Core.Enum;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        public void Add(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            SqliteDbContext dbContext = new SqliteDbContext();
            SqliteConnection connection = dbContext.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Projects (uuid, name,createdDate, startDate, endDate, description, status)" +
                "VALUES (@uuid, @name, @createdDate, @startDate, @endDate, @description, @status);";
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@name", entity.Name);
            command.Parameters.AddWithValue("@createdDate", entity.CreatedDate);
            command.Parameters.AddWithValue("@startDate", entity.StartDate);
            command.Parameters.AddWithValue("@endDate", entity.EndDate);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@status", entity.Status);
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

        public Project GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
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
                var project = new Project(
                                        Guid.Parse(reader["uuid"].ToString()),
                    reader["name"].ToString(),
                    reader.GetDateTime(reader.GetOrdinal("startDate")),
                    reader.GetDateTime(reader.GetOrdinal("endDate")),
                    reader["description"].ToString(),
                    Enum.TryParse<ProjectStatus>(reader["status"].ToString(), out var status)
                    ? status
                    : ProjectStatus.Planned, // default fallback value
                    reader.GetDateTime(reader.GetOrdinal("createdDate"))
                    );
                connection.Close();
                return project;
            }
            return null;
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
                var project = new Project(
                    Guid.Parse(reader["uuid"].ToString()),
                    reader["name"].ToString(),
                    reader.GetDateTime(reader.GetOrdinal("startDate")),
                    reader.GetDateTime(reader.GetOrdinal("endDate")),
                    reader["description"].ToString(),
                    Enum.TryParse<ProjectStatus>(reader["status"].ToString(), out var status) 
        ? status 
        : ProjectStatus.Planned, // default fallback value
                    reader.GetDateTime(reader.GetOrdinal("createdDate"))
                    );
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

            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@name", entity.Name);
            command.Parameters.AddWithValue("@startDate", entity.StartDate);
            command.Parameters.AddWithValue("@endDate", entity.EndDate);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@status", entity.Status);
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
            command.CommandText = "DELETE FROM projects WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid.ToString());
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}