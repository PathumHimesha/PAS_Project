using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    [Authorize(Roles = "ModuleLeader")] // Admin ta witharai mekata enna puluwan
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Mulu system ekema thiyena okkoma projects, students la saha supervisors la ekkama gannawa
            var allProjects = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .ToListAsync();

            return View(allProjects);
        }
    }
}