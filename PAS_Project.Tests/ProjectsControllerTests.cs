using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS_Project.Controllers;
using PAS_Project.Data;
using PAS_Project.Models;
using Xunit;

namespace PAS_Project.Tests
{
    public class ProjectsControllerTests
    {
        [Fact]
        public async Task ExpressInterest_ShouldUpdateStatusToMatched_AndAssignSupervisor()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // --- 1. ARRANGE ---
            using (var context = new ApplicationDbContext(options))
            {
                // Fix: Mulinma Dummy Student kenek database ekata danna ona
                var dummyStudent = new Student 
                { 
                    StudentId = 100, 
                    Name = "Test Student", 
                    Email = "test@student.com" 
                };
                context.Students.Add(dummyStudent);
                context.SaveChanges(); // Student wa save karanna

                // Dan Project eka hadaddi e Student wa link karanna
                context.Projects.Add(new Project 
                { 
                    ProjectId = 1, 
                    Title = "AI System", 
                    Abstract = "Test Abstract",
                    TechnicalStack = "C#",
                    ResearchArea = "AI",
                    Status = ProjectStatus.Pending,
                    StudentId = 100 // Link to dummy student
                });
                context.SaveChanges();
            }

            // --- 2. ACT ---
            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ProjectsController(context);
                
                // Supervisor (ID: 55) project ekata interest eka pennanawa
                var result = await controller.ExpressInterest(1, 55);

                // --- 3. ASSERT ---
                var updatedProject = await context.Projects.FindAsync(1);

                // Dan check karanna Matched wunada kiyala!
                Assert.NotNull(updatedProject);
                Assert.Equal(ProjectStatus.Matched, updatedProject.Status);
                Assert.Equal(55, updatedProject.SupervisorId);

                // Reveal page ekata Redirect wunada?
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("MatchDetails", redirectToActionResult.ActionName);
            }
        }
    }
}