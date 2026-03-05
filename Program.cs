using Microsoft.AspNetCore.HttpOverrides;
using NearbyFriendsApp.Services;

namespace NearbyFriendsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<LocationService>();
            var app = builder.Build();

            // Required for reverse-proxy deployments (Render, Railway, etc.)
            // so the app correctly reads X-Forwarded-For / X-Forwarded-Proto headers.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // HTTPS is terminated at the reverse proxy; do not redirect here.
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            app.Run($"http://0.0.0.0:{port}");
        }
    }
}
