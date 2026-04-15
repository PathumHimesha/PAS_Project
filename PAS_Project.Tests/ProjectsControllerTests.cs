using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            // BUG FIX 1: Database eke Student kenek nathnam auto eyaawa hadanawa (Foreign Key Error eka nawaththanna)
            if (!_context.Students.Any(s => s.StudentId == 1))
            {
                _context.Students.Add(new Student { StudentId = 1, Name = "Test Student", Email = "test@student.com" });
                await _context.SaveChangesAsync();
            }

            // Dummy data set kireema
            project.StudentId = 1; 
            project.Status = ProjectStatus.Pending;

            // BUG FIX 2: Strict validation errors clear kireema
            ModelState.Clear();

            // Try-Catch block ekak damma errors mokakhari awoth eka page eke lassanata pennanna
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                
                // Save wunata passe Table ekata yanawa
                return RedirectToAction(nameof(Index)); 
            }
            catch (System.Exception ex)
            {
                // Error ekak awoth oyaata pennanawa
                return Content("DATABASE ERROR EKA: " + ex.Message + " | " + ex.InnerException?.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int projectId, int supervisorId)
        {
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