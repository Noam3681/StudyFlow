
using Model;

namespace View.Services
{
    // שירות ששומר את פרטי המשתמש המחובר באתר.
    public class UserSession
    {
        // אפשר לקרוא את הפרטים מבחוץ,
        // אבל לשנות אותם רק דרך פעולות המחלקה.
        public int UserId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public int RoleId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;

        // משתמש נחשב מחובר אם יש לו מזהה גדול מאפס.
        public bool IsLoggedIn => UserId > 0;

        // אירוע שמודיע לרכיבי האתר שהמשתמש המחובר השתנה.
        public event Action? OnChange;

        // שמירת פרטי המשתמש לאחר התחברות מוצלחת.
        public void Login(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Email = user.Email;
            RoleId = user.RoleID;
            RoleName = user.Role;

            // מודיעים לרכיבים שצריך לעדכן את התצוגה.
            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }

        // ניתוק המשתמש וניקוי פרטי ההתחברות.
        public void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            Email = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;

            // מודיעים לרכיבים שהמשתמש התנתק.
            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }
    }
}

