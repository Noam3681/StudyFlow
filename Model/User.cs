using System.Text.Json.Serialization;

namespace Model
{
    // מייצג משתמש במערכת StudyFlow.
    // המשתמש שומר את מזהה התפקיד שלו,
    // ושם התפקיד מתקבל מבסיס הנתונים בעזרת JOIN עם טבלת Roles.
    public class User : BaseEntity
    {
        // שם המשתמש.
        public string Username { get; set; }

        // כתובת האימייל של המשתמש.
        public string Email { get; set; }

        // הסיסמה עדיין נשמרת בתוך העצם כדי שהמערכת תוכל לבדוק התחברות.
        // JsonIgnore מונע מהסיסמה להופיע בתשובות JSON שה-API מחזיר.
        [JsonIgnore]
        public string Password { get; set; }

        // המפתח הזר שמצביע על התפקיד בטבלת Roles.
        public int RoleID { get; set; }

        // שם התפקיד, למשל Student או Admin.
        // הערך מתקבל מ-JOIN עם טבלת Roles.
        public string Role { get; set; }

        // האם המשתמש פעיל במערכת.
        public bool IsActive { get; set; }

        // מועד יצירת המשתמש.
        public DateTime CreatedAt { get; set; }


        // בנאי למשתמש חדש שעדיין לא נשמר בבסיס הנתונים.
        public User(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;

            // משתמש חדש מקבל כברירת מחדל את תפקיד Student.
            // בטבלת Roles קבענו ש-RoleID 2 הוא Student.
            RoleID = 2;
            Role = "Student";

            IsActive = true;
        }


        // בנאי למשתמש שנקרא מבסיס הנתונים.
        // מקבלים גם RoleID וגם את שם התפקיד שהגיע מה-JOIN.
        public User(
            int id,
            string username,
            string email,
            string password,
            int roleID,
            string role,
            bool isActive,
            DateTime createdAt)
            : base(id)
        {
            Username = username;
            Email = email;
            Password = password;

            RoleID = roleID;
            Role = role;

            IsActive = isActive;
            CreatedAt = createdAt;
        }


        // הצגה קצרה של המשתמש לצורכי בדיקה ודיבוג.
        public override string ToString()
        {
            return $"{Username} - {Role}";
        }
    }
}