
using MySql.Data.MySqlClient;

namespace DAL
{
    // אחראית על יצירת בסיס הנתונים, הטבלאות ונתוני ההתחלה של StudyFlow.
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "StudyFlowDB";

        public static async Task InitializeAsync(string connectionString)
        {
            // קודם יוצרים את בסיס הנתונים והטבלאות.
            await CreateDatabaseAsync(connectionString);
            await CreateTablesAsync(connectionString);

            // קודם יוצרים את התפקידים, ורק אחר כך משתמשים ששייכים אליהם.
            await SeedRolesAsync(connectionString);
            await SeedUsersAsync(connectionString);
        }

        private static async Task CreateDatabaseAsync(string connectionString)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql =
                $"CREATE DATABASE IF NOT EXISTS `{DatabaseName}`;";

            using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTablesAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            string[] commands =
            {
                // Roles
                // טבלה נפרדת לתפקידי המשתמשים.
                "CREATE TABLE IF NOT EXISTS `Roles` (" +
                "`RoleID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`RoleName` VARCHAR(20) NOT NULL UNIQUE" +
                ");",

                // Users
                // כל משתמש שומר RoleID שמפנה לטבלת Roles.
                "CREATE TABLE IF NOT EXISTS `Users` (" +
                "`UserID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`Username` VARCHAR(50) NOT NULL UNIQUE, " +
                "`Email` VARCHAR(100) NOT NULL UNIQUE, " +
                "`PasswordHash` VARCHAR(255) NOT NULL, " +
                "`RoleID` INT NOT NULL DEFAULT 2, " +
                "`IsActive` BOOLEAN DEFAULT TRUE, " +
                "`CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                "FOREIGN KEY (`RoleID`) REFERENCES `Roles`(`RoleID`)" +
                ");",

                // Subjects
                "CREATE TABLE IF NOT EXISTS `Subjects` (" +
                "`SubjectID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`SubjectName` VARCHAR(80) NOT NULL UNIQUE" +
                ");",

                // UserSubjects
                "CREATE TABLE IF NOT EXISTS `UserSubjects` (" +
                "`UserID` INT NOT NULL, " +
                "`SubjectID` INT NOT NULL, " +
                "`AddedAt` DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                "PRIMARY KEY (`UserID`, `SubjectID`), " +
                "FOREIGN KEY (`UserID`) REFERENCES `Users`(`UserID`), " +
                "FOREIGN KEY (`SubjectID`) REFERENCES `Subjects`(`SubjectID`)" +
                ");",

                // AcademicItems
                "CREATE TABLE IF NOT EXISTS `AcademicItems` (" +
                "`ItemID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`UserID` INT NOT NULL, " +
                "`SubjectID` INT NOT NULL, " +
                "`Title` VARCHAR(150) NOT NULL, " +
                "`Description` TEXT NULL, " +
                "`ItemType` VARCHAR(20) NOT NULL, " +
                "`DueDate` DATETIME NOT NULL, " +
                "`Priority` VARCHAR(20) DEFAULT 'Medium', " +
                "`Status` VARCHAR(20) DEFAULT 'Pending', " +
                "`CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                "`CompletedAt` DATETIME NULL, " +
                "FOREIGN KEY (`UserID`) REFERENCES `Users`(`UserID`), " +
                "FOREIGN KEY (`SubjectID`) REFERENCES `Subjects`(`SubjectID`)" +
                ");",

                // TimetableEntries
                "CREATE TABLE IF NOT EXISTS `TimetableEntries` (" +
                "`TimetableEntryID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`UserID` INT NOT NULL, " +
                "`SubjectID` INT NOT NULL, " +
                "`DayOfWeek` INT NOT NULL, " +
                "`StartTime` TIME NOT NULL, " +
                "`EndTime` TIME NOT NULL, " +
                "`Location` VARCHAR(100) NULL, " +
                "`Notes` TEXT NULL, " +
                "FOREIGN KEY (`UserID`) REFERENCES `Users`(`UserID`), " +
                "FOREIGN KEY (`SubjectID`) REFERENCES `Subjects`(`SubjectID`)" +
                ");",

                // StudyMaterials
                "CREATE TABLE IF NOT EXISTS `StudyMaterials` (" +
                "`MaterialID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`UserID` INT NOT NULL, " +
                "`SubjectID` INT NOT NULL, " +
                "`Title` VARCHAR(150) NOT NULL, " +
                "`MaterialType` VARCHAR(30) NOT NULL, " +
                "`MaterialUrl` VARCHAR(500) NULL, " +
                "`Content` TEXT NULL, " +
                "`CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP, " +
                "FOREIGN KEY (`UserID`) REFERENCES `Users`(`UserID`), " +
                "FOREIGN KEY (`SubjectID`) REFERENCES `Subjects`(`SubjectID`)" +
                ");",

                // StudySessions
                "CREATE TABLE IF NOT EXISTS `StudySessions` (" +
                "`StudySessionID` INT AUTO_INCREMENT PRIMARY KEY, " +
                "`UserID` INT NOT NULL, " +
                "`SubjectID` INT NOT NULL, " +
                "`ItemID` INT NULL, " +
                "`StudyDate` DATE NOT NULL, " +
                "`StartTime` TIME NOT NULL, " +
                "`EndTime` TIME NOT NULL, " +
                "`Notes` TEXT NULL, " +
                "`IsCompleted` BOOLEAN DEFAULT FALSE, " +
                "FOREIGN KEY (`UserID`) REFERENCES `Users`(`UserID`), " +
                "FOREIGN KEY (`SubjectID`) REFERENCES `Subjects`(`SubjectID`), " +
                "FOREIGN KEY (`ItemID`) REFERENCES `AcademicItems`(`ItemID`)" +
                ");"
            };

            // מריצים כל פקודת CREATE TABLE בנפרד.
            foreach (string sql in commands)
            {
                using var command = new MySqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        // מוסיף את התפקידים הבסיסיים של המערכת.
        // ID 1 = Admin, ID 2 = Student.
        private static async Task SeedRolesAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            string sql =
                "INSERT IGNORE INTO Roles (RoleID, RoleName) " +
                "VALUES " +
                "(1, 'Admin'), " +
                "(2, 'Student');";

            using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        // מוסיף משתמשי התחלה רק אם טבלת Users עדיין ריקה.
        private static async Task SeedUsersAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            // בודקים אם כבר קיימים משתמשים בבסיס הנתונים.
            string countSql = "SELECT COUNT(*) FROM Users;";

            using var countCommand =
                new MySqlCommand(countSql, connection);

            int count =
                Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // לא מוסיפים משתמשי התחלה אם הטבלה אינה ריקה.
            if (count > 0)
            {
                return;
            }

            // פקודת SQL להוספת משתמש עם גיבוב סיסמה.
            string sql =
                "INSERT INTO Users " +
                "(Username, Email, PasswordHash, RoleID, IsActive) " +
                "VALUES " +
                "(@Username, @Email, @Hash, " +
                "(SELECT RoleID FROM Roles WHERE RoleName = @RoleName), TRUE);";

            // יוצרים משתמש מנהל ושומרים גיבוב במקום סיסמה רגילה.
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", "admin");
                command.Parameters.AddWithValue("@Email", "admin@studyflow.com");
                command.Parameters.AddWithValue("@Hash",
                    BCrypt.Net.BCrypt.HashPassword("admin"));
                command.Parameters.AddWithValue("@RoleName", "Admin");

                await command.ExecuteNonQueryAsync();
            }

            // יוצרים משתמש תלמיד ושומרים גיבוב במקום סיסמה רגילה.
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Username", "user");
                command.Parameters.AddWithValue("@Email", "user@studyflow.com");
                command.Parameters.AddWithValue("@Hash",
                    BCrypt.Net.BCrypt.HashPassword("user"));
                command.Parameters.AddWithValue("@RoleName", "Student");

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
