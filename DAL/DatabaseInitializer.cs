using MySql.Data.MySqlClient;

namespace DAL
{
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "StudyFlowDB";

        public static async Task InitializeAsync(string connectionString)
        {
            await CreateDatabaseAsync(connectionString);
            await CreateTablesAsync(connectionString);
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
                // Users
                """
                CREATE TABLE IF NOT EXISTS `Users`
                (
                    `UserID` INT AUTO_INCREMENT PRIMARY KEY,
                    `Username` VARCHAR(50) NOT NULL UNIQUE,
                    `Email` VARCHAR(100) NOT NULL UNIQUE,
                    `PasswordHash` VARCHAR(255) NOT NULL,
                    `Role` VARCHAR(20) NOT NULL DEFAULT 'Student',
                    `IsActive` BOOLEAN DEFAULT TRUE,
                    `CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                """,

                // Subjects
                """
                CREATE TABLE IF NOT EXISTS `Subjects`
                (
                    `SubjectID` INT AUTO_INCREMENT PRIMARY KEY,
                    `SubjectName` VARCHAR(80) NOT NULL UNIQUE
                );
                """,

                // UserSubjects
                """
                CREATE TABLE IF NOT EXISTS `UserSubjects`
                (
                    `UserID` INT NOT NULL,
                    `SubjectID` INT NOT NULL,
                    `AddedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,

                    PRIMARY KEY (`UserID`, `SubjectID`),

                    FOREIGN KEY (`UserID`)
                        REFERENCES `Users`(`UserID`),

                    FOREIGN KEY (`SubjectID`)
                        REFERENCES `Subjects`(`SubjectID`)
                );
                """,

                // AcademicItems
                """
                CREATE TABLE IF NOT EXISTS `AcademicItems`
                (
                    `ItemID` INT AUTO_INCREMENT PRIMARY KEY,
                    `UserID` INT NOT NULL,
                    `SubjectID` INT NOT NULL,
                    `Title` VARCHAR(150) NOT NULL,
                    `Description` TEXT NULL,
                    `ItemType` VARCHAR(20) NOT NULL,
                    `DueDate` DATETIME NOT NULL,
                    `Priority` VARCHAR(20) DEFAULT 'Medium',
                    `Status` VARCHAR(20) DEFAULT 'Pending',
                    `CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,
                    `CompletedAt` DATETIME NULL,

                    FOREIGN KEY (`UserID`)
                        REFERENCES `Users`(`UserID`),

                    FOREIGN KEY (`SubjectID`)
                        REFERENCES `Subjects`(`SubjectID`)
                );
                """,

                // TimetableEntries
                """
                CREATE TABLE IF NOT EXISTS `TimetableEntries`
                (
                    `TimetableEntryID` INT AUTO_INCREMENT PRIMARY KEY,
                    `UserID` INT NOT NULL,
                    `SubjectID` INT NOT NULL,
                    `DayOfWeek` INT NOT NULL,
                    `StartTime` TIME NOT NULL,
                    `EndTime` TIME NOT NULL,
                    `Location` VARCHAR(100) NULL,
                    `Notes` TEXT NULL,

                    FOREIGN KEY (`UserID`)
                        REFERENCES `Users`(`UserID`),

                    FOREIGN KEY (`SubjectID`)
                        REFERENCES `Subjects`(`SubjectID`)
                );
                """,

                // StudyMaterials
                """
                CREATE TABLE IF NOT EXISTS `StudyMaterials`
                (
                    `MaterialID` INT AUTO_INCREMENT PRIMARY KEY,
                    `UserID` INT NOT NULL,
                    `SubjectID` INT NOT NULL,
                    `Title` VARCHAR(150) NOT NULL,
                    `MaterialType` VARCHAR(30) NOT NULL,
                    `MaterialUrl` VARCHAR(500) NULL,
                    `Content` TEXT NULL,
                    `CreatedAt` DATETIME DEFAULT CURRENT_TIMESTAMP,

                    FOREIGN KEY (`UserID`)
                        REFERENCES `Users`(`UserID`),

                    FOREIGN KEY (`SubjectID`)
                        REFERENCES `Subjects`(`SubjectID`)
                );
                """,

                // StudySessions
                """
                CREATE TABLE IF NOT EXISTS `StudySessions`
                (
                    `StudySessionID` INT AUTO_INCREMENT PRIMARY KEY,
                    `UserID` INT NOT NULL,
                    `SubjectID` INT NOT NULL,
                    `ItemID` INT NULL,
                    `StudyDate` DATE NOT NULL,
                    `StartTime` TIME NOT NULL,
                    `EndTime` TIME NOT NULL,
                    `Notes` TEXT NULL,
                    `IsCompleted` BOOLEAN DEFAULT FALSE,

                    FOREIGN KEY (`UserID`)
                        REFERENCES `Users`(`UserID`),

                    FOREIGN KEY (`SubjectID`)
                        REFERENCES `Subjects`(`SubjectID`),

                    FOREIGN KEY (`ItemID`)
                        REFERENCES `AcademicItems`(`ItemID`)
                );
                """
            };

            foreach (string sql in commands)
            {
                using var command = new MySqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task SeedUsersAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString)
            {
                Database = DatabaseName
            };

            using var connection =
                new MySqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            string countSql = "SELECT COUNT(*) FROM Users;";

            using var countCommand =
                new MySqlCommand(countSql, connection);

            int count =
                Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            if (count > 0)
            {
                return;
            }

            string sql = """
                INSERT INTO Users
                (Username, Email, PasswordHash, Role, IsActive)
                VALUES
                ('admin', 'admin@studyflow.com', 'Admin123', 'Admin', TRUE),
                ('student1', 'student1@studyflow.com', 'Student123', 'Student', TRUE),
                ('student2', 'student2@studyflow.com', 'Student123', 'Student', TRUE),
                ('student3', 'student3@studyflow.com', 'Student123', 'Student', TRUE),
                ('student4', 'student4@studyflow.com', 'Student123', 'Student', TRUE),
                ('student5', 'student5@studyflow.com', 'Student123', 'Student', TRUE),
                ('student6', 'student6@studyflow.com', 'Student123', 'Student', TRUE),
                ('student7', 'student7@studyflow.com', 'Student123', 'Student', TRUE),
                ('student8', 'student8@studyflow.com', 'Student123', 'Student', TRUE),
                ('student9', 'student9@studyflow.com', 'Student123', 'Student', TRUE);
                """;

            using var command =
                new MySqlCommand(sql, connection);

            await command.ExecuteNonQueryAsync();
        }
    }
}