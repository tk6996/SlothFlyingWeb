using Microsoft.EntityFrameworkCore;
using SlothFlyingWeb.Models;

namespace SlothFlyingWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
    }
}