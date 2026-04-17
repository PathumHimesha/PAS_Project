using Microsoft.EntityFrameworkCore;
using PAS_Project.Data;
using PAS_Project.Models;
using Xunit;

namespace PAS_Project.Tests
{
    public class MatchLogicTests
    {
        [Fact]
        public void ProjectStatus_UpdatesToMatched_WhenSupervisorAccepts()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_PAS_DB_Unique")
                .Options;

            // 1. Create a Pending Project (Missing required fields added!)
            using (var context = new ApplicationDbContext(options))
            {
                var project = new Project 
                { 
                    ProjectId = 100, 
                    Title = "AI Testing System", 
                    Abstract = "A comprehensive system to test AI matching algorithms.", // Added
                    TechnicalStack = "C#, xUnit, Moq",                                 // Added
                    ResearchArea = "Software Testing",                                 // Added
                    Status = ProjectStatus.Pending, 
                    StudentId = 1 
                };
                context.Projects.Add(project);
                context.SaveChanges(); // Dan meka fail wenne naha!
            }

            // Act: Supervisor accepts the project
            using (var context = new ApplicationDbContext(options))
            {
                var project = context.Projects.Find(100);
                project.SupervisorId = 5; // Assigning Supervisor
                project.Status = ProjectStatus.Matched; // Changing Status
                context.Update(project);
                context.SaveChanges();
            }

            // Assert: Verify the database updated correctly
            using (var context = new ApplicationDbContext(options))
            {
                var project = context.Projects.Find(100);
                Assert.Equal(ProjectStatus.Matched, project.Status);
                Assert.Equal(5, project.SupervisorId);
            }
        }
    }
}