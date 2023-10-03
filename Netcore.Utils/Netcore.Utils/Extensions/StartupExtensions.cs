using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetCore.Utils.Cache;
using NetCore.Utils.Database;
using NetCore.Utils.Filters;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Sessions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Utils.Extensions
{
    public static class StartupExtensions
    {
        /// <summary>
        ///  Những project có webapi và signalr
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="hubName"></param>
        /// <returns></returns>
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
                            //NLogManager.LogInfo(accessToken);
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

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR(hubOptions =>
            {
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(300);
                hubOptions.EnableDetailedErrors = true;
                //hubOptions.SupportedProtocols = "WebSockets";
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver =
                new DefaultContractResolver();
            });

            services.AddCollections();
            return services;
        }

        /// <summary>
        ///  Những project chỉ có webapi
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiCommon(this IServiceCollection services, string key)
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
                });

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCollections();
            return services;
        }

        public static IServiceCollection AddCollections(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<AccountSession>();
            services.AddTransient<CacheHandler>();
            services.AddTransient<IDBHelper, DBHelper>();
            services.AddTransient<ISecurity, Security.Security>();
            services.AddTransient<ClientIdCheckFilter>();
            services.AddHttpClient<IDataService, DataService>();
            return services;
        }

        public static IApplicationBuilder UseExtentions(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature.Error;

                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }));
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            return app;
        }
    }
}