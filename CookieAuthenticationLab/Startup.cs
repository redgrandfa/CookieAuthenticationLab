using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CookieAuthenticationLab
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
            services.AddControllersWithViews();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    //設定登入Action的路徑： 
                    options.LoginPath = new PathString("/Account/Login");

                    //設定 導回網址 的QueryString參數名稱：
                    options.ReturnUrlParameter = "ReturnUrl";

                    //設定登出Action的路徑： 
                    options.LogoutPath = new PathString("/Account/Logout");

                    //若權限不足，會導向的Action的路徑
                    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SomePolicyName", policy =>
                    policy
                        .RequireRole("A")
                        .RequireClaim(ClaimTypes.Name) //要求具有指定的ClaimType
                        .RequireClaim("age", "18", "19") //要求具有age這個ClaimType，且值是18或19
                );
                options.AddPolicy("OO", policy => 
                    policy.RequireClaim(ClaimTypes.Name)
                );
                options.AddPolicy("XX", policy => 
                    policy.RequireClaim(ClaimTypes.Name)
                );
            });

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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
