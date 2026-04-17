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
    [Authorize] // Login wela inna ayata witharai access
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. VIEW PROJECTS (DASHBOARDS)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            
            int userId = int.Parse(userIdClaim.Value);

            if (User.IsInRole("Supervisor"))
            {
                var allProjects = await _context.Projects.Include(p => p.Student).ToListAsync();
                return View(allProjects);
            }
            
            var myProjects = await _context.Projects
                .Include(p => p.Student)
                .Where(p => p.StudentId == userId)
                .ToListAsync();
            
            return View(myProjects);
        }

        // ==========================================
        // 2. STUDENT: CREATE PROJECT
        // ==========================================
        public IActionResult Create()
        {
            if (!User.IsInRole("Student")) return Forbid();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            project.StudentId = int.Parse(userIdClaim.Value);
            project.Status = ProjectStatus.Pending;

            // BUG FIX: Bypass strict validation to prevent 400 Errors
            ModelState.Clear();

            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. STUDENT: EDIT PROJECT
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || !User.IsInRole("Student")) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.Status != ProjectStatus.Pending) return Forbid(); 

            var userId = int.Parse(User.FindFirst("UserId").Value);
            if (project.StudentId != userId) return Forbid();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.ProjectId) return NotFound();

            var userId = int.Parse(User.FindFirst("UserId").Value);
            project.StudentId = userId;
            project.Status = ProjectStatus.Pending;

            ModelState.Clear();
            _context.Update(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. STUDENT: WITHDRAW (DELETE) PROJECT
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || !User.IsInRole("Student")) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.Status != ProjectStatus.Pending) return Forbid();

            var userId = int.Parse(User.FindFirst("UserId").Value);
            if (project.StudentId != userId) return Forbid();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. SUPERVISOR: THE BLIND MATCH ENGINE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ExpressInterest(int projectId)
        {
            if (!User.IsInRole("Supervisor")) return Forbid();

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