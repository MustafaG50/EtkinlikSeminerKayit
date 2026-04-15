using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Application.Services;
using EtkinlikSeminerKayit.Persistence.Context;
using EtkinlikSeminerKayit.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace EtkinlikSeminerKayit.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Veritabaný Baðlantýsý
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // 2. Repository ve UnitOfWork Kayýtlarý
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Servis Kayýtlarý (Application Katmanýndan)
            builder.Services.AddScoped<IReservationService, ReservationService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");        
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
