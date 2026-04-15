using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PAS_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Login Page eka pennana method eka
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login form eka submit kalama wada karana method eka
        [HttpPost]
        public async Task<IActionResult> Login(string email, string role)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var claims = new List<Claim>();

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
                if (student == null) {
                    // Email eka nathnam aluth student kenek auto hadenawa
                    student = new Models.Student { Name = email.Split('@')[0], Email = email };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }
                claims.Add(new Claim(ClaimTypes.Name, student.Name));
                claims.Add(new Claim("UserId", student.StudentId.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, "Student"));
            }
            else if (role == "Supervisor")
            {
                var supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Email == email);
                if (supervisor == null) {
                    // Email eka nathnam aluth supervisor kenek auto hadenawa
                    supervisor = new Models.Supervisor { Name = "Dr. " + email.Split('@')[0], Email = email, PreferredResearchAreas = "Software Engineering" };
                    _context.Supervisors.Add(supervisor);
                    await _context.SaveChangesAsync();
                }
                claims.Add(new Claim(ClaimTypes.Name, supervisor.Name));
                claims.Add(new Claim("UserId", supervisor.SupervisorId.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, "Supervisor"));
            }

            // ASP.NET Core eke cookie eka hadanawa (Login wenawa)
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            // Login wunata passe yanna ona thana
            if (role == "Supervisor") return RedirectToAction("Dashboard", "Supervisors");
            return RedirectToAction("Index", "Projects");
        }

        // Logout wenna
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}