using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using ServerCore.Utilities.Database;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Sessions;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSignalrCommon(this IServiceCollection services, string key, string hubName)
        {
            var keyByte = Encoding.ASCII.GetBytes(key);

            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            LifetimeValidator = (before, expires, token, param) =>
                            {
                                return expires > DateTime.UtcNow;
                            },
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = new SymmetricSecurityKey(keyByte)
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments(hubName)))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR(hubOptions =>
            {
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(300);
                hubOptions.EnableDetailedErrors = true;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver =
                new DefaultContractResolver();
            });

            services.AddHttpContextAccessor();
            services.AddSingleton<AccountSession>();
            //services.AddSingleton<IAccountSession, AccountSession>();
            //services.AddSingleton<IDBHelper, DBHelper>();
            return services;
        }

        public static IApplicationBuilder UseExtentions(this IApplicationBuilder app)
        {
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            //AccountSession.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}