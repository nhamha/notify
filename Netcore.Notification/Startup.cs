using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Netcore.Notification.Controllers;
using Netcore.Notification.DataAccess;
using Netcore.Notification.Hubs;
using Netcore.Notification.Models;
using NetCore.Utils;
using NetCore.Utils.Extensions;
using NetCore.Utils.Interfaces;

namespace Netcore.Notification
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            services.AddSignalrCommon(appSettings.Secret, "/gateHub");
            services.AddSingleton<ConnectionHandler>();
            services.AddSingleton<PlayerHandler>();
            services.AddSingleton<NotificationHandler>();
            services.AddSingleton<JackpotController>();
            services.AddSingleton<XJackpotController>();
            services.AddSingleton<EventController>();
            services.AddSingleton<SQLAccess>();
            services.AddSingleton<JobEventAccess>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseExtentions();
            app.UseSignalR(routes =>
            {
                routes.MapHub<GateHub>("/gateHub");
            });
        }
    }
}