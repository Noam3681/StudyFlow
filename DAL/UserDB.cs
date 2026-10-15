using System.Data.Common;
using Model;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class UserDB
    {
        private readonly string connectionString;

        public UserDB(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<int> AddUserAsync(User user)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                INSERT INTO Users
                    (Username, Email, PasswordHash, Role, IsActive)
                VALUES
                    (@Username, @Email, @Password, @Role, @IsActive);
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            await command.ExecuteNonQueryAsync();

            return (int)command.LastInsertedId;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT COUNT(*) FROM Users WHERE Username = @Username;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", username);

            int count =
                Convert.ToInt32(await command.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT COUNT(*) FROM Users WHERE Email = @Email;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Email", email);

            int count =
                Convert.ToInt32(await command.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<User?> CheckLoginAsync(
            string username,
            string password)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                SELECT
                    UserID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    IsActive,
                    CreatedAt
                FROM Users
                WHERE Username = @Username
                  AND IsActive = TRUE;
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", username);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            User user = ReadUser(reader);

            if (user.Password != password)
            {
                return null;
            }

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                SELECT
                    UserID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    IsActive,
                    CreatedAt
                FROM Users
                WHERE UserID = @UserID;
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@UserID", id);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return ReadUser(reader);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                SELECT
                    UserID,
                    Username,
                    Email,
                    PasswordHash,
                    Role,
                    IsActive,
                    CreatedAt
                FROM Users
                ORDER BY UserID;
                """;

            using var command = new MySqlCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(ReadUser(reader));
            }

            return users;
        }

        public async Task<Dictionary<string, int>>
            CountUsersByRoleAsync()
        {
            Dictionary<string, int> result =
                new Dictionary<string, int>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                SELECT
                    Role,
                    COUNT(*) AS UserCount
                FROM Users
                GROUP BY Role;
                """;

            using var command = new MySqlCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string role =
                    Convert.ToString(reader["Role"])!;

                int count =
                    Convert.ToInt32(reader["UserCount"]);

                result[role] = count;
            }

            return result;
        }

        public async Task<bool> UpdateUserRoleAsync(
            int userId,
            string role)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                UPDATE Users
                SET Role = @Role
                WHERE UserID = @UserID;
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Role", role);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> UpdatePasswordAsync(
            int userId,
            string password)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                UPDATE Users
                SET PasswordHash = @Password
                WHERE UserID = @UserID;
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> UpdateEmailAsync(
            int userId,
            string email)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = """
                UPDATE Users
                SET Email = @Email
                WHERE UserID = @UserID;
                """;

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        private static User ReadUser(DbDataReader reader)
        {
            return new User(
                Convert.ToInt32(reader["UserID"]),
                Convert.ToString(reader["Username"])!,
                Convert.ToString(reader["Email"])!,
                Convert.ToString(reader["PasswordHash"])!,
                Convert.ToString(reader["Role"])!,
                Convert.ToBoolean(reader["IsActive"]),
                Convert.ToDateTime(reader["CreatedAt"])
            );
        }
    }
}