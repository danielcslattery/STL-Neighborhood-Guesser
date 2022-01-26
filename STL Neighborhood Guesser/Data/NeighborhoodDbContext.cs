using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using STL_Neighborhood_Guesser.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public List<Neighborhood> CheckClickLocation(double lon, double lat)
        {
            List<Neighborhood> response = Neighborhoods
                .FromSqlRaw("SELECT * " +
                "FROM neighborhoods " +
                "WHERE st_contains(geodata, ST_SRID(Point ({0}, {1}), 4326));", lon, lat)
                .ToList();

            return response;
        }
    }
}
