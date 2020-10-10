using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPKODABbl.Service;

namespace OPKODABbl
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebsiteContext>(options => options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Orcodav_WebsiteDB;Trusted_Connection=True;"));
            services.AddDbContext<UsersContext>(options => options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Orcodav_UsersDB;Trusted_Connection=True;"));
            services.AddDbContext<ForumContext>(options => options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Orcodav_ForumDB;Trusted_Connection=True;"));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Account/Login");
                });

            services.AddAuthorization(a =>
            {
                a.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
            });

            services.AddControllersWithViews(a =>
            {
                a.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));
            });//.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Main}/{action=Index}/{Id?}");
            });
        }
    }
}
