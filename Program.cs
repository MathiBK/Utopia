using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using utopia.Data;
using utopia.Helper;

namespace utopia
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var env = services.GetService<IWebHostEnvironment>();
                
                    try
                    {
                        using (var context = new ApplicationDbContext(
                            services.GetRequiredService<
                                DbContextOptions<ApplicationDbContext>>()))
                        {

                            if (env.IsDevelopment())
                            {
                                MapDbInitializer.Initialize(context, env.IsDevelopment());
                                AnimalDbInitializer.Initialize(context, env.IsDevelopment(), services.GetService<IWebHostEnvironment>());
                                ResourceDbInitializer.Initialize(context, env.IsDevelopment());
                                TribeDbInitializer.Initialize(context, env.IsDevelopment(), services.GetService<IWebHostEnvironment>());
                                ItemDbInitializer.Initialize(context, env.IsDevelopment());
                                ItemCraftingDbInitializer.Initialize(context, env.IsDevelopment());
                            }
                            else
                            {
                                context.Database.Migrate();

                                if (context.Tiles.FirstOrDefault(t => t.Id == 1) == default)
                                {

                                    MapDbInitializer.Initialize(context, env.IsDevelopment());
                                }

                                if (context.SpeciesIndividuals.FirstOrDefault(t => t.Id == 1) == default)
                                {
                                    AnimalDbInitializer.Initialize(context, env.IsDevelopment(), services.GetService<IWebHostEnvironment>());
                                }

                                if (context.Resources.FirstOrDefault(t => t.Id == 1) == default)
                                {

                                    ResourceDbInitializer.Initialize(context, env.IsDevelopment());
                                }
                                if (context.Tribes.FirstOrDefault(t => t.Id == 1) == default)
                                {
                                    TribeDbInitializer.Initialize(context, env.IsDevelopment(), services.GetService<IWebHostEnvironment>());
                                }
                                if (context.Items.FirstOrDefault(t => t.Id == 1) == default)
                                {
                                    ItemDbInitializer.Initialize(context,env.IsDevelopment());                                
                                }
                                if (context.ItemCrafting.FirstOrDefault(t => t.Id == 1) == default)
                                {
                                    ItemCraftingDbInitializer.Initialize(context, env.IsDevelopment());
                                    
                                }

                                context.SaveChanges();
                                
                            }
                        }

                    }

                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred seeding the DB.");
                    }
                
            }

            host.Run();
        }
        

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
