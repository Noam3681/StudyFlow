using DAL;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace StudyFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserDB userDB;

        public UsersController(UserDB userDB)
        {
            this.userDB = userDB;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            return Ok(await userDB.GetAllUsersAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await userDB.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("username-exists")]
        public async Task<ActionResult<bool>> UsernameExists(
            [FromQuery] string username)
        {
            return Ok(await userDB.UsernameExistsAsync(username));
        }

        [HttpGet("email-exists")]
        public async Task<ActionResult<bool>> EmailExists(
            [FromQuery] string email)
        {
            return Ok(await userDB.EmailExistsAsync(email));
        }

        [HttpPost]
        public async Task<ActionResult> AddUser(CreateUserRequest request)
        {
            if (await userDB.UsernameExistsAsync(request.Username))
                return Conflict("Username already exists.");

            if (await userDB.EmailExistsAsync(request.Email))
                return Conflict("Email already exists.");

            var user =
                new User(request.Username, request.Email, request.Password);

            int id = await userDB.AddUserAsync(user);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id },
                new { id });
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginRequest request)
        {
            var user = await userDB.CheckLoginAsync(
                request.Username,
                request.Password);

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }

        [HttpGet("count-by-role")]
        public async Task<ActionResult<Dictionary<string, int>>> CountUsersByRole()
        {
            return Ok(await userDB.CountUsersByRoleAsync());
        }

        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(
            int id,
            UpdateRoleRequest request)
        {
            if (request.Role != "Student" &&
                request.Role != "Admin")
            {
                return BadRequest("Invalid role.");
            }

            bool updated =
                await userDB.UpdateUserRoleAsync(id, request.Role);

            if (!updated)
                return NotFound();

            return NoContent();
        }

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

        [HttpPut("{id:int}/email")]
        public async Task<IActionResult> UpdateEmail(
            int id,
            UpdateEmailRequest request)
        {
            if (await userDB.EmailExistsAsync(request.Email))
                return Conflict("Email already exists.");

            bool updated =
                await userDB.UpdateEmailAsync(id, request.Email);

            if (!updated)
                return NotFound();

            return NoContent();
        }
    }

    public record CreateUserRequest(
        string Username,
        string Email,
        string Password);

    public record LoginRequest(
        string Username,
        string Password);

    public record UpdateRoleRequest(string Role);

    public record UpdatePasswordRequest(string Password);

    public record UpdateEmailRequest(string Email);
}