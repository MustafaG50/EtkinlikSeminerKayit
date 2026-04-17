using EtkinlikSeminerKayit.Application.DTOs;
using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace EtkinlikSeminerKayit.WebApp.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationController(
            IReservationService reservationService, IUnitOfWork unitOfWork)
        {
            _reservationService = reservationService;
            _unitOfWork = unitOfWork;
        }

        //Rezervasyon Yapma Ekranı
        public async Task<IActionResult> Create()
        {
            // Ekranda seçilmesi için Salonları ve Etkinlik Tiplerini getiriyoruz
            ViewBag.Resources = await _unitOfWork.Repository<Resource>().GetAllAsync();
            ViewBag.EventTypes = await _unitOfWork.Repository<EventType>().GetAllAsync();

            return View();
        }

        // Kaydet butonuna basılınca çalışır.
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için ekledik.
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            var result = await _reservationService.CreateReservationAsync(dto);

            // İşlem başarılı olsada olmasada salon ve etkinlik tiplerini dolduruyoruz.
            ViewBag.Resources = await _unitOfWork.Repository<Resource>().GetAllAsync();
            ViewBag.EventTypes = await _unitOfWork.Repository<EventType>().GetAllAsync();

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                // Kutucukları temizlemek için boş dto gönderilir.
                return View(new CreateReservationDto());
            }

            // Hata varsa mesajı göster ve kullanıcının yazdığı verileri (dto) geri gönder
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        [HttpGet] // Seçilen türe göre dinamik alanları getirir.
        public async Task<IActionResult> GetFieldsByEventType(int eventTypeId)
        {
            var fields = await _unitOfWork.Repository<EventField>()
              .FindAsync(f => f.EventTypeId == eventTypeId);

            return Json(fields.Select(f => new { f.Id, f.Name, f.DataType }));
        }
        public async Task<IActionResult> Index()
        {
            var reservations = await _unitOfWork.Repository<Reservation>().GetAllAsync(
                include: q => q.Include(r => r.Resource)
                               .Include(r => r.EventType)
                               .Include(r => r.EventValues)
                                    .ThenInclude(ev => ev.EventField)
            );
            return View(reservations.OrderBy(r => r.StartTime));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Önce rezervasyonu ve bağlı değerleri çekiyoruz
            var reservations = await _unitOfWork.Repository<Reservation>().GetAllAsync(
                filter: r => r.Id == id,
                include: q => q.Include(r => r.EventValues)
            );

            var entity = reservations.FirstOrDefault();

            // 2. Nesne boş mu kontrol et
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Silinecek kayıt bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // 3. ŞİMDİ kontrolü yapabiliriz (Hata buradaydı, entity artık dolu)
            if (entity.StartTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş tarihteki kayıtlar silinemez.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Önce bağlı EAV değerlerini sil
                foreach (var val in entity.EventValues.ToList())
                {
                    _unitOfWork.Repository<EventValue>().Delete(val);
                }

                // Sonra ana rezervasyonu sil
                _unitOfWork.Repository<Reservation>().Delete(entity);

                await _unitOfWork.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervasyon başarıyla iptal edildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        // Bu fonksiyon salona koltuk eklemek için kullanılır.
        // Dinamik olarak 'Koltuk No' alanını bulur ve o koltuğun o saatte dolu olup olmadığını kontrol eder.
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için ekledik
        public async Task<IActionResult> AddSeat(int resourceId, int eventTypeId, DateTime start, DateTime end, string seatNo)
        {
            if (start < DateTime.Now.AddMinutes(-5))
            {
                TempData["ErrorMessage"] = $"TARİH HATASI! Seçilen: {start:dd.MM.yyyy HH:mm:ss} | Sunucu Saati: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
                return RedirectToAction(nameof(Index));
            }

            var allFieldsForThisType = await _unitOfWork.Repository<EventField>()
                .FindAsync(f => f.EventTypeId == eventTypeId);

            var seatField = allFieldsForThisType.FirstOrDefault(f => f.Name.Contains("Koltuk"))
                ?? allFieldsForThisType.FirstOrDefault()
                ?? (await _unitOfWork.Repository<EventField>().FindAsync(f => f.Name.Contains("Koltuk"))).FirstOrDefault();

            if (seatField == null)
            {
                TempData["ErrorMessage"] = "Koltuk numarası alanı sistemde tanımlı değil.";
                return RedirectToAction(nameof(Index));
            }

            // Aynı koltuk dolumu kontrolü
            var isBusy = await _unitOfWork.Repository<Reservation>().AnyAsync(r =>
                r.ResourceId == resourceId &&
                r.StartTime == start &&
                r.EventValues.Any(v => v.Value == seatNo));

            if (isBusy)
            {
                TempData["ErrorMessage"] = "Bu koltuk az önce doldu.";
                return RedirectToAction(nameof(Index));
            }

            var newReservation = new Reservation
            {
                ResourceId = resourceId,
                EventTypeId = eventTypeId,
                StartTime = start,
                EndTime = end,
                EventValues = new List<EventValue>
                {
                    new EventValue {
                    EventFieldId = seatField.Id,
                    Value = seatNo
                    }
                }
            };

            await _unitOfWork.Repository<Reservation>().AddAsync(newReservation);
            await _unitOfWork.SaveChangesAsync(); // DB ekleme işlemi.

            TempData["SuccessMessage"] = $"Koltuk {seatNo} başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Salona ait tüm kayıtları silen fonksiyon
        public async Task<IActionResult> DeleteBySalon(int resourceId, DateTime startTime)
        {
            // Bağlı verileri silmek için include ile birlikte rezervasyonları çekiyoruz
            var reservationsToDelete = await _unitOfWork.Repository<Reservation>()
                .GetAllAsync(
                    filter: r => r.ResourceId == resourceId && r.StartTime == startTime,
                    include: q => q.Include(r => r.EventValues)
                );

            if (reservationsToDelete != null && reservationsToDelete.Any())
            {
                try
                {
                    foreach (var res in reservationsToDelete)
                    {
                     
                        if (res.EventValues != null)
                        {
                            foreach (var val in res.EventValues.ToList())
                            {
                                _unitOfWork.Repository<EventValue>().Delete(val);
                            }
                        }

                        _unitOfWork.Repository<Reservation>().Delete(res);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Salonun bu saatteki tüm rezervasyonları temizlendi.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Silme işlemi sırasında bir hata oluştu: " +
                        (ex.InnerException?.Message ?? ex.Message);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
    