namespace Model
{
    public class User : BaseEntity
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }


        // משתמש חדש שעדיין לא נשמר בבסיס הנתונים
        public User(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;

            Role = "Student";
            IsActive = true;
        }


        // משתמש שנקרא מבסיס הנתונים
        public User(
            int id,
            string username,
            string email,
            string password,
            string role,
            bool isActive,
            DateTime createdAt)
            : base(id)
        {
            Username = username;
            Email = email;
            Password = password;
            Role = role;
            IsActive = isActive;
            CreatedAt = createdAt;
        }


        public override string ToString()
        {
            return $"{Username} - {Role}";
        }
    }
}