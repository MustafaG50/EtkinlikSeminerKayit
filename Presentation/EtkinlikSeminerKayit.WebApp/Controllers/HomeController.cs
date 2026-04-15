using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Entities;
using EtkinlikSeminerKayit.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EtkinlikSeminerKayit.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // HATA 2 ÇÖZÜMÜ: _unitOfWork deðiþkenini burada tanýmlýyoruz
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanýndan verileri çekiyoruz
            var resources = await _unitOfWork.Repository<Resource>().GetAllAsync();
            var reservations = await _unitOfWork.Repository<Reservation>().GetAllAsync();

            // 1. Toplam Salon Sayýsý
            ViewBag.TotalResources = resources.Count();

            // 2. Toplam Kayýt (Koltuk bazlý tüm baþvurular)
            ViewBag.TotalReservations = reservations.Count();

            // 3. Aktif Etkinlikler (Sadece benzersiz Salon + Saat seanslarýný sayar)
            ViewBag.ActiveReservations = reservations
                .Where(r => r.EndTime > DateTime.Now) // Henüz bitmemiþ olanlar
                .GroupBy(r => new { r.ResourceId, r.StartTime }) // Ayný salon ve saat seanslarýný grupla
                .Count(); // Gruplarýn sayýsýný al (Yani benzersiz seans sayýsý)

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
