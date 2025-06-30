using Microsoft.EntityFrameworkCore;
using CsvProcessorApp.Models;

namespace CsvProcessorApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ProcessingResult> ProcessingResults { get; set; }
        public DbSet<User> Users { get; set; } 
        
    }
}
