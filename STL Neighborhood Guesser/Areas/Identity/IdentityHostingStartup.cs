using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STL_Neighborhood_Guesser.Data;

[assembly: HostingStartup(typeof(STL_Neighborhood_Guesser.Areas.Identity.IdentityHostingStartup))]
namespace STL_Neighborhood_Guesser.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<NeighborhoodDbContext>(options =>
                    options.UseMySql(
                        context.Configuration.GetConnectionString("DefaultConnection")));

                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddEntityFrameworkStores<NeighborhoodDbContext>();
            });
        }
    }
}