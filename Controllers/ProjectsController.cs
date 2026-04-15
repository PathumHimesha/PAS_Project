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

        // ==========================================
        // 1. STUDENT DASHBOARD
        // ==========================================

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
            if (!_context.Students.Any(s => s.StudentId == 1))
            {
                _context.Students.Add(new Student { StudentId = 1, Name = "Test Student", Email = "test@student.com" });
                await _context.SaveChangesAsync();
            }

            project.StudentId = 1; 
            project.Status = ProjectStatus.Pending;

            ModelState.Clear();

            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); 
            }
            catch (System.Exception ex)
            {
                return Content("DATABASE ERROR EKA: " + ex.Message + " | " + ex.InnerException?.Message);
            }
        }

        // ==========================================
        // 2. THE BLIND MATCH ENGINE (Supervisor Action)
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> ExpressInterest(int projectId, int supervisorId)
        {
            // BUG FIX: Added 'PreferredResearchAreas' to prevent NOT NULL constraint error
            if (!_context.Supervisors.Any(s => s.SupervisorId == supervisorId))
            {
                _context.Supervisors.Add(new Supervisor { 
                    SupervisorId = supervisorId, 
                    Name = "Dr. Default Supervisor", 
                    Email = "dr.supervisor@pas.com",
                    PreferredResearchAreas = "Software Engineering, AI" // <--- MEKA THAMAI ALUTHIN DAMME
                });
                await _context.SaveChangesAsync();
            }

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