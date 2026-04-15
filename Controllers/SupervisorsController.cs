using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    public class SupervisorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var projects = await _context.Projects
                .Include(p => p.Student)
                .ToListAsync();

            return View(projects);
        }
    }
}