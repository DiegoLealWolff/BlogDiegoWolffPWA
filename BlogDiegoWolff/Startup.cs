﻿using System;
using BlogDiegoWolff.Services;
using BlogDiegoWolff.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogDiegoWolff
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
                
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {                
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddPushSubscriptionStore(Configuration)
                .AddPushNotificationService(Configuration);

            services.AddSingleton<IBlogService, BlogService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");                
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) => {
                    var header = context.Context.Response.GetTypedHeaders();

                    header.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });
            app.UseCookiePolicy();
            app.UseRouting();
            
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                PushSubscriptionContext context = serviceScope.ServiceProvider.GetService<PushSubscriptionContext>();
                context.Database.EnsureCreated();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
