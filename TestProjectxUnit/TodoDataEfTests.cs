using DataAccessLibrary.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestProjectxUnit
{
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }
        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(_options, null!);
        }
        public ValueTask<ApplicationDbContext> CreateDbContextAsync()
        {
            return new ValueTask<ApplicationDbContext>(new ApplicationDbContext(_options, null!));
        }
    }

    public class TodoDataEfTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TodoDataEfTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Create schema
            using var context = new ApplicationDbContext(_options, null!);
            context.Database.EnsureCreated();

            // Seed data
            context.Todos.AddRange(
                new Todo { Title = "Buy milk", Description = "From the shop", Project = "Home", Completed = false, Archived = false, SortPriority = 10 },
                new Todo { Title = "Fix bug", Description = "Fix the login bug", Project = "Work", Completed = false, Archived = false, SortPriority = 5 },
                new Todo { Title = "Old", Description = "Archived item", Project = "Home", Completed = true, Archived = true, SortPriority = 1 }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetTodos_ReturnsFilteredAndOrdered()
        {
            var factory = new TestDbContextFactory(_options);
            var repo = new DataAccessLibrary.TodoDataEf(factory);

            var all = await repo.GetTodos();
            Assert.Equal(2, all.Count);

            var search = await repo.GetTodos("milk", null);
            Assert.Single(search);
            Assert.Equal("Buy milk", search.First().Title);

            var project = await repo.GetTodos(null, "Work");
            Assert.Single(project);
            Assert.Equal("Fix bug", project.First().Title);
        }

        [Fact]
        public async Task GetProjects_ReturnsDistinctProjects()
        {
            var factory = new TestDbContextFactory(_options);
            var repo = new DataAccessLibrary.TodoDataEf(factory);

            var projects = await repo.GetProjects();
            Assert.Contains("Home", projects);
            Assert.Contains("Work", projects);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
