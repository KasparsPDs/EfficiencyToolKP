using System;
using EfficiencyToolKPlocal.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(EfficiencyToolKPlocal.Areas.Identity.IdentityHostingStartup))]
namespace EfficiencyToolKPlocal.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<EfficiencyToolKPlocalContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("EfficiencyToolKPlocalContextConnection")));

                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<EfficiencyToolKPlocalContext>();
            });
        }
    }
}