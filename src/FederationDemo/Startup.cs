using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Owin;

namespace FederationDemo {
    public class Startup {
        public static IConfiguration config { get; set; }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");

            config = builder.Build();
            
            // set up Nancy OWIN hosting
            app.UseOwin(x => x.UseNancy(options => {
                options.Bootstrapper = new AppBootstrapper(config);
            }));
            
        }
    }
}