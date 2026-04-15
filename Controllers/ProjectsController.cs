using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;

namespace PAS_Project.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- The "Blind Match" Logic ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int projectId, int supervisorId)
        {
            var project = await _context.Projects
                                        .Include(p => p.Student) 
                                        .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null) return NotFound();

            // 1. Assign the Supervisor
            project.SupervisorId = supervisorId; 

            // 2. Change the Status to Matched
            project.Status = ProjectStatus.Matched;

            _context.Update(project);
            await _context.SaveChangesAsync();

            // 3. Redirect to The "Reveal" Page
            return RedirectToAction("MatchDetails", new { id = project.ProjectId }); 
        }

        // --- The Reveal Page Logic ---
        public async Task<IActionResult> MatchDetails(int? id)
        {
            if (id == null) return NotFound();

            // Fetch project WITH Student and Supervisor data included
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