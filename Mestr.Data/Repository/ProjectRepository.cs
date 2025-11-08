using Mestr.Data.DbContext;
using Mestr.Data.Interface;
using Microsoft.Data.Sqlite;
using Mestr.Core.Model;
using Mestr.Core.Enum;

namespace Mestr.Data.Repository
{
    public class ProjectRepository : IRepository<Project>
    {
        private readonly SqliteDbContext _dbContext;
        private readonly SqliteConnection _connection;
        public ProjectRepository() {
            _dbContext = SqliteDbContext.Instance;
            _connection = _dbContext.GetConnection();
        }
        public void Add(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO Projects (uuid, name,createdDate, startDate, endDate, description, status)" +
                "VALUES (@uuid, @name, @createdDate, @startDate, @endDate, @description, @status);";
            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@name", entity.Name);
            command.Parameters.AddWithValue("@createdDate", entity.CreatedDate);
            command.Parameters.AddWithValue("@startDate", entity.StartDate);
            command.Parameters.AddWithValue("@endDate", entity.EndDate);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@status", entity.Status.ToString());
            command.ExecuteNonQuery();
        }

        public Project? GetByUuid(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM projects WHERE uuid = @uuid";
            
            command.Parameters.AddWithValue("@uuid", uuid);

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                var project = new Project(
                    Guid.Parse(reader["uuid"].ToString()!),
                    reader["name"].ToString()!,
                    reader.GetDateTime(reader.GetOrdinal("createdDate")),
                    reader.GetDateTime(reader.GetOrdinal("startDate")),
                    reader["description"].ToString()!,
                    Enum.TryParse<ProjectStatus>(reader["status"].ToString(), out var status)
                    ? status
                    : ProjectStatus.Planned, // default fallback value
                    reader.GetDateTime(reader.GetOrdinal("endDate"))
                    );
                return project;
            }
            return null;
        }

        public IEnumerable<Project> GetAll()
        {
            var projects = new List<Project>();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM Projects";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var project = new Project(
                    Guid.Parse(reader["uuid"].ToString()!),
                    reader["name"].ToString()!,
                    reader.GetDateTime(reader.GetOrdinal("createdDate")),
                    reader.GetDateTime(reader.GetOrdinal("startDate")),
                    reader["description"].ToString()!,
                    Enum.TryParse<ProjectStatus>(reader["status"].ToString(), out var status) 
        ? status 
        : ProjectStatus.Planned, // default fallback value
                    reader.GetDateTime(reader.GetOrdinal("endDate"))
                    );
                projects.Add(project);
            }
            return projects;
        }

        public void Update(Project entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE projects " +
                "SET name = @name, " +
                "startDate = @startDate, " +
                "endDate = @endDate, " +
                "description = @description, " +
                "status = @status " +
                "WHERE uuid = @uuid";

            command.Parameters.AddWithValue("@uuid", entity.Uuid);
            command.Parameters.AddWithValue("@name", entity.Name);
            command.Parameters.AddWithValue("@startDate", entity.StartDate);
            command.Parameters.AddWithValue("@endDate", entity.EndDate);
            command.Parameters.AddWithValue("@description", entity.Description);
            command.Parameters.AddWithValue("@status", entity.Status.ToString());
            command.ExecuteNonQuery();

        }

        public void Delete(Guid uuid)
        {
            if (uuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(uuid));
            }
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM projects WHERE uuid = @uuid";
            command.Parameters.AddWithValue("@uuid", uuid);
            command.ExecuteNonQuery();
        }
    }
}