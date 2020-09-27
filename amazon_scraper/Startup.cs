using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amazon_scraper.Databases;
using amazon_scraper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace amazon_scraper
{
    public class Startup
    {
        // Yes, I know it's bad to copy this from Program but I'm writing that piece of code at midnight
        private const string DB_DATA_SOURCE_CONNECTION_STRING = "DataSource=AmazonScraper.db";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole(c => c.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ")
                    .AddConfiguration(Configuration)
                    .SetMinimumLevel(LogLevel.Information);
            });

            services.AddSingleton<IReviewRepository>(sp => new ReviewRepository(DB_DATA_SOURCE_CONNECTION_STRING, sp.GetRequiredService<ILogger<ReviewRepository>>()));
            services.AddSingleton<IReviewIndexerService, ReviewIndexerService>();
            services.AddSingleton<IScrapingService, ScrapingService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
