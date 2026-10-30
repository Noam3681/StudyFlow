using DAL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace StudyFlowAPI.Controllers
{
    // Controller שאחראי על כל הבקשות הקשורות למשתמשים.
    // הוא מקבל בקשות HTTP מה-View או מקובץ ה-.http,
    // ופונה ל-UserDB כדי לבצע את הפעולות מול בסיס הנתונים.
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserDB userDB;

        // מקבל את UserDB דרך Dependency Injection.
        // כך ה-Controller לא יוצר בעצמו חיבור לבסיס הנתונים.
        public UsersController(UserDB userDB)
        {
            this.userDB = userDB;
        }


        // מחזיר את כל המשתמשים במערכת.
        // GET api/users
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            return Ok(await userDB.GetAllUsersAsync());
        }


        // מחזיר משתמש מסוים לפי ה-ID שלו.
        // GET api/users/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await userDB.GetUserByIdAsync(id);

            // אם לא נמצא משתמש עם ה-ID שנשלח, מחזירים 404.
            if (user == null)
                return NotFound();

            return Ok(user);
        }


        // בודק האם שם משתמש כבר קיים במערכת.
        // הערך מתקבל דרך Query String.
        // לדוגמה:
        // GET api/users/username-exists?username=admin
        [HttpGet("username-exists")]
        public async Task<ActionResult<bool>> UsernameExists(
            [FromQuery] string username)
        {
            return Ok(await userDB.UsernameExistsAsync(username));
        }


        // בודק האם כתובת אימייל כבר קיימת במערכת.
        // לדוגמה:
        // GET api/users/email-exists?email=admin@studyflow.com
        [HttpGet("email-exists")]
        public async Task<ActionResult<bool>> EmailExists(
            [FromQuery] string email)
        {
            return Ok(await userDB.EmailExistsAsync(email));
        }


        // מוסיף משתמש חדש למערכת.
        // POST api/users
        [HttpPost]
        public async Task<ActionResult> AddUser(CreateUserRequest request)
        {
            // מונעים יצירת משתמש עם שם משתמש שכבר קיים.
            if (await userDB.UsernameExistsAsync(request.Username))
                return Conflict("Username already exists.");

            // מונעים יצירת משתמש עם אימייל שכבר קיים.
            if (await userDB.EmailExistsAsync(request.Email))
                return Conflict("Email already exists.");

            // יוצרים עצם User חדש מהפרטים שהתקבלו בבקשה.
            var user =
                new User(request.Username, request.Email, request.Password);

            // שומרים את המשתמש בבסיס הנתונים ומקבלים את ה-ID החדש.
            int id = await userDB.AddUserAsync(user);

            // מחזירים 201 Created יחד עם ה-ID של המשתמש שנוצר.
            return CreatedAtAction(
                nameof(GetUserById),
                new { id },
                new { id });
        }


        // בודק פרטי התחברות של משתמש.
        // POST api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginRequest request)
        {
            // UserDB בודק אם שם המשתמש והסיסמה מתאימים.
            var user = await userDB.CheckLoginAsync(
                request.Username,
                request.Password);

            // אם הפרטים לא נכונים, מחזירים 401 Unauthorized.
            if (user == null)
                return Unauthorized();

            // אם ההתחברות הצליחה, מחזירים את המשתמש.
            // הסיסמה לא מוחזרת ב-JSON בגלל JsonIgnore במחלקת User.
            return Ok(user);
        }


        // מחזיר כמה משתמשים קיימים בכל תפקיד.
        // GET api/users/count-by-role
        [HttpGet("count-by-role")]
        public async Task<ActionResult<Dictionary<string, int>>> CountUsersByRole()
        {
            return Ok(await userDB.CountUsersByRoleAsync());
        }


        // משנה את התפקיד של משתמש קיים.
        // PUT api/users/{id}/role
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(
            int id,
            UpdateRoleRequest request)
        {
            // מאפשרים רק את התפקידים שקיימים במערכת.
            if (request.Role != "Student" &&
                request.Role != "Admin")
            {
                return BadRequest("Invalid role.");
            }

            // UserDB מוצא את ה-RoleID המתאים בטבלת Roles
            // ומעדכן אותו אצל המשתמש.
            bool updated =
                await userDB.UpdateUserRoleAsync(id, request.Role);

            // אם המשתמש לא נמצא, מחזירים 404.
            if (!updated)
                return NotFound();

            // העדכון הצליח ואין צורך להחזיר תוכן.
            return NoContent();
        }


        // משנה את הסיסמה של משתמש קיים.
        // PUT api/users/{id}/password
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> UpdatePassword(
            int id,
            UpdatePasswordRequest request)
        {
            bool updated =
                await userDB.UpdatePasswordAsync(
                    id,
                    request.Password);

            if (!updated)
                return NotFound();

            return NoContent();
        }


        // משנה את כתובת האימייל של משתמש קיים.
        // PUT api/users/{id}/email
        [HttpPut("{id:int}/email")]
        public async Task<IActionResult> UpdateEmail(
            int id,
            UpdateEmailRequest request)
        {
            // לא מאפשרים לעדכן לאימייל שכבר קיים אצל משתמש אחר.
            if (await userDB.EmailExistsAsync(request.Email))
                return Conflict("Email already exists.");

            bool updated =
                await userDB.UpdateEmailAsync(id, request.Email);

            if (!updated)
                return NotFound();

            return NoContent();
        }
    }


    // הנתונים שנדרשים ליצירת משתמש חדש.
    public class CreateUserRequest
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }


    // הנתונים שנדרשים להתחברות.
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }


    // הנתונים שנדרשים לשינוי תפקיד.
    public class UpdateRoleRequest
    {
        public string Role { get; set; } = "";
    }


    // הנתונים שנדרשים לשינוי סיסמה.
    public class UpdatePasswordRequest
    {
        public string Password { get; set; } = "";
    }


    // הנתונים שנדרשים לשינוי אימייל.
    public class UpdateEmailRequest
    {
        public string Email { get; set; } = "";
    }
}