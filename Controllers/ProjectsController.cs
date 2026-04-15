using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    [Authorize] // <--- Login wenne nathuwa mekata enna baha!
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            // Aththa log wechcha student ge ID eka gannawa
            var userId = int.Parse(User.FindFirst("UserId").Value);

            project.StudentId = userId; 
            project.Status = ProjectStatus.Pending;

            ModelState.Clear();

            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); 
        }

        [HttpPost]
        public async Task<IActionResult> ExpressInterest(int projectId)
        {
            // Aththa log wechcha supervisor ge ID eka gannawa
            var supervisorId = int.Parse(User.FindFirst("UserId").Value);

            var project = await _context.Projects
                                        .Include(p => p.Student) 
                                        .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null) return NotFound();

            project.SupervisorId = supervisorId; 
            project.Status = ProjectStatus.Matched;

            _context.Update(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("MatchDetails", new { id = project.ProjectId }); 
        }

        public async Task<IActionResult> MatchDetails(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(m => m.ProjectId == id);

            if (project == null || project.Status != ProjectStatus.Matched)
            {
                return NotFound("Project not found or not yet matched.");
            }

            return View(project);
        }
    }
}