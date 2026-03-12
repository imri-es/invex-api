using System.Security.Claims;
using invex_api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace invex_api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users/search?q=...
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new List<object>());

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = q.ToLower();

            var users = await _context
                .Users.Where(u =>
                    u.Id != currentUserId
                    && (
                        u.Email!.ToLower().Contains(query)
                        || u.UserName!.ToLower().Contains(query)
                        || u.FullName.ToLower().Contains(query)
                    )
                )
                .Take(10)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
