using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Host.Handlers;
using ServiceStack.Logging;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;

namespace RazorRockstars.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseServiceStack(new AppHost {
                AppSettings = new NetCoreAppSettings(Configuration)
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(new RazorHandler("/notfound"));

            //Other examples of using built-in ServiceStack Handlers as middleware
            //app.Use(new StaticFileHandler("wwwroot/img/react-logo.png").Middleware);
            //app.Use(new RequestInfoHandler().Middleware);
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Test Razor", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig { DebugMode = true });

            Plugins.Add(new RazorFormat());

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().Open())
            {
                db.CreateTableIfNotExists<Rockstar>();
                db.InsertAll(RockstarsService.SeedData);
            }
        }
    }
}
