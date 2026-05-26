using Microsoft.EntityFrameworkCore;
using GCAMS.Models;


namespace GCAMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Students> Students { get; set; }
    }
}
    