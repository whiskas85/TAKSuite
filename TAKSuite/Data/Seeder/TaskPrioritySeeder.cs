namespace TAKSuite.Data.Seeder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using TAKSuite.Data.Models;

    public class TaskPrioritySeeder : ISeeder
    {
        private readonly ApplicationDbContext _context;

        public TaskPrioritySeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            _context.TaskPriorities.RemoveRange(_context.TaskPriorities);
            _context.TaskPriorities.AddRange(
                new TaskPriority
                {
                    Id = Guid.NewGuid(),
                    Name = "None",
                    Level = 0,
                    CardColor = BlazorBootstrap.CardColor.Secondary
                },
                new TaskPriority
                {
                    Id = Guid.NewGuid(),
                    Name = "Low",
                    Level = 1,
                    CardColor = BlazorBootstrap.CardColor.Success
                },
                new TaskPriority
                {
                    Id = Guid.NewGuid(),
                    Name = "Medium",
                    Level = 2,
                    CardColor = BlazorBootstrap.CardColor.Warning
                },
                new TaskPriority
                {
                    Id = Guid.NewGuid(),
                    Name = "High",
                    Level = 3,
                    CardColor = BlazorBootstrap.CardColor.Danger
                }
             );

            await _context.SaveChangesAsync();
        }

       
    }
}



