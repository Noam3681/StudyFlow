
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

        // V8: מוסיף את שלושת התפקידים של מערכת StudyFlow.
        // ID 1 = Admin, ID 2 = Student, ID 3 = Teacher.
        private static async Task SeedRolesAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            // INSERT IGNORE מונע הוספה חוזרת של תפקידים קיימים.
            // ב-V8 הוספנו את Teacher בלי לשנות את התפקידים הקודמים.
            string sql =
                "INSERT IGNORE INTO Roles (RoleID, RoleName) " +
                "VALUES " +
                "(1, 'Admin'), " +
                "(2, 'Student'), " +
                "(3, 'Teacher');";

            using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        // V8: מוסיף משתמש בדיקה לכל תפקיד, רק אם הוא עדיין לא קיים.
        private static async Task SeedUsersAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            // בודקים ומוסיפים משתמשי בדיקה, בלי לשנות משתמשים קיימים.
            await AddTestUserIfMissingAsync(
                connection, "admin", "admin@studyflow.com", "admin", "Admin");

            await AddTestUserIfMissingAsync(
                connection, "user", "user@studyflow.com", "user", "Student");

            // V8: משתמש בדיקה חדש עבור תפקיד המורה.
            await AddTestUserIfMissingAsync(
                connection, "teacher", "teacher@studyflow.com", "teacher", "Teacher");
        }

        // V8: מוסיף משתמש בדיקה רק אם שם המשתמש והאימייל אינם קיימים.
        // הסיסמה נשמרת כגיבוב BCrypt ולא כטקסט רגיל.
        private static async Task AddTestUserIfMissingAsync(
            MySqlConnection connection,
            string username,
            string email,
            string password,
            string roleName)
        {
            // בודקים אם שם המשתמש או האימייל כבר קיימים.
            string checkSql =
                "SELECT COUNT(*) FROM Users " +
                "WHERE Username = @Username OR Email = @Email;";

            using (var checkCommand = new MySqlCommand(checkSql, connection))
            {
                checkCommand.Parameters.AddWithValue("@Username", username);
                checkCommand.Parameters.AddWithValue("@Email", email);

                int count =
                    Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                // אם המשתמש כבר קיים, לא משנים את פרטיו.
                if (count > 0)
                {
                    return;
                }
            }

            // יוצרים גיבוב BCrypt עבור סיסמת משתמש הבדיקה.
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(password);

            // מוסיפים את המשתמש עם התפקיד המתאים מטבלת Roles.
            string insertSql =
                "INSERT INTO Users " +
                "(Username, Email, PasswordHash, RoleID, IsActive) " +
                "VALUES " +
                "(@Username, @Email, @Hash, " +
                "(SELECT RoleID FROM Roles WHERE RoleName = @RoleName), TRUE);";

            using var command = new MySqlCommand(insertSql, connection);

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Hash", passwordHash);
            command.Parameters.AddWithValue("@RoleName", roleName);

            await command.ExecuteNonQueryAsync();
        }
    }
}
