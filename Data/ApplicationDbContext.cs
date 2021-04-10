using Microsoft.EntityFrameworkCore;
using SlothFlyingWeb.Models;

namespace SlothFlyingWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }

        public DbSet<Lab> Lab { get; set; }

        public DbSet<BookList> BookList { get; set; }

        public DbSet<BookSlot> BookSlot { get; set; }

        public DbSet<Admin> Admin { get; set; }
    }
}