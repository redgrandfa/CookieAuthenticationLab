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
                    //�]�w�n�JAction�����|�G 
                    options.LoginPath = new PathString("/Account/Login");

                    //�]�w �ɦ^���} ��QueryString�ѼƦW�١G
                    options.ReturnUrlParameter = "ReturnUrl";

                    //�]�w�n�XAction�����|�G 
                    options.LogoutPath = new PathString("/Account/Logout");

                    //�Y�v�������A�|�ɦV��Action�����|
                    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SomePolicyName", policy =>
                    policy
                        .RequireRole("A")
                        .RequireClaim(ClaimTypes.Name) //�n�D�㦳���w��ClaimType
                        .RequireClaim("age", "18", "19") //�n�D�㦳age�o��ClaimType�A�B�ȬO18��19
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
