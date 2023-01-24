
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(utopia.Areas.Identity.IdentityHostingStartup))]
namespace utopia.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}