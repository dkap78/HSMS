using HSMS.DAL;
using HSMS.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.API.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly HSMSDbContext _db;

        public RolesController(HSMSDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> List()
            => Ok(await _db.Roles.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            var id = await _db.Roles.CreateAsync(role);
            return Ok(new { id });
        }
    }
}
