using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using STL_Neighborhood_Guesser.Models;

namespace STL_Neighborhood_Guesser.Data
{
    public class NeighborhoodDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<Score> Scores { get; set; }

        public NeighborhoodDbContext(DbContextOptions<NeighborhoodDbContext> options)
        : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }


}
