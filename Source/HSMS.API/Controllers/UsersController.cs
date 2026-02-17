using HSMS.DAL;
using HSMS.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public UsersController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _db.Users.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user.ToDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var id = await _db.Users.CreateAsync(user);
            return CreatedAtAction(nameof(Get), new { id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, User user)
        {
            user.Id = id;
            await _db.Users.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _db.Users.SoftDeleteAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var users = await _db.Users.SearchAsync(q);
            return Ok(users.Select(x => x.ToDto()));
        }
    }
}
