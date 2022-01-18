using Microsoft.EntityFrameworkCore;
using STL_Neighborhood_Guesser.Models;

namespace STL_Neighborhood_Guesser.Data
{
    public class NeighborhoodDbContext : DbContext
    {
        public DbSet<Neighborhood> Neighborhoods { get; set; }

        public NeighborhoodDbContext(DbContextOptions<NeighborhoodDbContext> options)
        : base(options)
        {
        }
    }
}
