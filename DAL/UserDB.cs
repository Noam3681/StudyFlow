using System.Data.Common;
using Model;
using MySql.Data.MySqlClient;

namespace DAL
{
    // מחלקה שאחראית על כל הפעולות של משתמשים מול בסיס הנתונים.
    // אין כאן לוגיקה של תצוגה או API - רק עבודה עם MySQL.
    public class UserDB
    {
        private readonly string connectionString;

        // מקבל את מחרוזת החיבור לבסיס הנתונים.
        public UserDB(string connectionString)
        {
            this.connectionString = connectionString;
        }


        // מוסיף משתמש חדש לטבלת Users ומחזיר את המזהה שנוצר לו.
        public async Task<int> AddUserAsync(User user)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // התפקיד נשמר כ-RoleID ולא כטקסט.
            string sql =
                "INSERT INTO Users " +
                "(Username, Email, PasswordHash, RoleID, IsActive) " +
                "VALUES " +
                "(@Username, @Email, @Password, @RoleID, @IsActive);";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@RoleID", user.RoleID);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            await command.ExecuteNonQueryAsync();

            return (int)command.LastInsertedId;
        }


        // בודק האם שם משתמש כבר קיים במערכת.
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


        // בודק האם כתובת אימייל כבר קיימת במערכת.
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


        // בודק פרטי כניסה של משתמש.
        // משתמשים ב-JOIN כדי לקבל את שם התפקיד מטבלת Roles.
        public async Task<User?> CheckLoginAsync(
            string username,
            string password)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT " +
                "u.UserID, " +
                "u.Username, " +
                "u.Email, " +
                "u.PasswordHash, " +
                "u.RoleID, " +
                "r.RoleName AS Role, " +
                "u.IsActive, " +
                "u.CreatedAt " +
                "FROM Users u " +
                "INNER JOIN Roles r ON u.RoleID = r.RoleID " +
                "WHERE u.Username = @Username " +
                "AND u.IsActive = TRUE;";

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


        // מחזיר משתמש לפי המזהה שלו.
        // שם התפקיד מגיע מטבלת Roles בעזרת JOIN.
        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT " +
                "u.UserID, " +
                "u.Username, " +
                "u.Email, " +
                "u.PasswordHash, " +
                "u.RoleID, " +
                "r.RoleName AS Role, " +
                "u.IsActive, " +
                "u.CreatedAt " +
                "FROM Users u " +
                "INNER JOIN Roles r ON u.RoleID = r.RoleID " +
                "WHERE u.UserID = @UserID;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@UserID", id);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return ReadUser(reader);
        }


        // מחזיר את כל המשתמשים במערכת.
        // JOIN מאפשר להחזיר את שם התפקיד במקום רק את RoleID.
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT " +
                "u.UserID, " +
                "u.Username, " +
                "u.Email, " +
                "u.PasswordHash, " +
                "u.RoleID, " +
                "r.RoleName AS Role, " +
                "u.IsActive, " +
                "u.CreatedAt " +
                "FROM Users u " +
                "INNER JOIN Roles r ON u.RoleID = r.RoleID " +
                "ORDER BY u.UserID;";

            using var command = new MySqlCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(ReadUser(reader));
            }

            return users;
        }


        // סופר כמה משתמשים קיימים בכל תפקיד.
        // שם התפקיד נלקח מטבלת Roles בעזרת JOIN.
        public async Task<Dictionary<string, int>> CountUsersByRoleAsync()
        {
            Dictionary<string, int> result =
                new Dictionary<string, int>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "SELECT " +
                "r.RoleName AS Role, " +
                "COUNT(*) AS UserCount " +
                "FROM Users u " +
                "INNER JOIN Roles r ON u.RoleID = r.RoleID " +
                "GROUP BY r.RoleID, r.RoleName;";

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


        // משנה את התפקיד של משתמש.
        // מקבלים את שם התפקיד, מוצאים את RoleID המתאים בטבלת Roles,
        // ושומרים את המזהה בטבלת Users.
        public async Task<bool> UpdateUserRoleAsync(
            int userId,
            string role)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "UPDATE Users " +
                "SET RoleID = (" +
                "SELECT RoleID " +
                "FROM Roles " +
                "WHERE RoleName = @Role" +
                ") " +
                "WHERE UserID = @UserID;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Role", role);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }


        // משנה את הסיסמה של משתמש קיים.
        public async Task<bool> UpdatePasswordAsync(
            int userId,
            string password)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "UPDATE Users " +
                "SET PasswordHash = @Password " +
                "WHERE UserID = @UserID;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }


        // משנה את כתובת האימייל של משתמש קיים.
        public async Task<bool> UpdateEmailAsync(
            int userId,
            string email)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                "UPDATE Users " +
                "SET Email = @Email " +
                "WHERE UserID = @UserID;";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@UserID", userId);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }


        // הופך את השורה שחזרה מ-MySQL לעצם User.
        // RoleID מגיע מ-Users ושם התפקיד מגיע מ-Roles דרך ה-JOIN.
        private static User ReadUser(DbDataReader reader)
        {
            return new User(
                Convert.ToInt32(reader["UserID"]),
                Convert.ToString(reader["Username"])!,
                Convert.ToString(reader["Email"])!,
                Convert.ToString(reader["PasswordHash"])!,
                Convert.ToInt32(reader["RoleID"]),
                Convert.ToString(reader["Role"])!,
                Convert.ToBoolean(reader["IsActive"]),
                Convert.ToDateTime(reader["CreatedAt"])
            );
        }
    }
}