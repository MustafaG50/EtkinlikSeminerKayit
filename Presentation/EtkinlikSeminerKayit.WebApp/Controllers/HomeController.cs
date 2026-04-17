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
        
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // DB den verileri çekiyoruz
            var resources = await _unitOfWork.Repository<Resource>().GetAllAsync();
            var reservations = await _unitOfWork.Repository<Reservation>().GetAllAsync();

            // Toplam Salon Sayýsý
            ViewBag.TotalResources = resources.Count();

            // Toplam Kayýt bütün koltuklarýn toplamý
            ViewBag.TotalReservations = reservations.Count();

            // Aktif etkinlikler bitmemiþ olanlar ve sadece benzersiz salon ve saat seanslarýný sayýyoruz
            ViewBag.ActiveReservations = reservations
                .Where(r => r.EndTime > DateTime.Now) // bitmemiþ olanlar
                .GroupBy(r => new { r.ResourceId, r.StartTime }) // Ayný salon ve saat seanslarýný grupla
                .Count(); // Gruplarýn sayýsýný al 

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
