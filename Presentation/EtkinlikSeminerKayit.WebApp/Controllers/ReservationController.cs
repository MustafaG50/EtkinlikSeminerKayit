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

        // 1. Rezervasyon Yapma Ekranı (Form)
        public async Task<IActionResult> Create()
        {
            // Formda seçilmesi için Salonları ve Etkinlik Tiplerini getiriyoruz
            ViewBag.Resources = await _unitOfWork.Repository<Domain.Entities.Resource>().GetAllAsync();
            ViewBag.EventTypes = await _unitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();

            return View();
        }

        // 2. Kaydet Butonuna Basıldığında Çalışan Metot
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            var result = await _reservationService.CreateReservationAsync(dto);

            // İşlem ister başarılı ister başarısız olsun, sayfayı tekrar yükleyeceğimiz için 
            // dropdown listelerini her durumda tekrar doldurmalıyız.
            ViewBag.Resources = await _unitOfWork.Repository<Resource>().GetAllAsync();
            ViewBag.EventTypes = await _unitOfWork.Repository<EventType>().GetAllAsync();

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                // Başarılı olduğunda formu temizlemek için yeni bir DTO gönderiyoruz
                return View(new CreateReservationDto());
            }

            // Hata varsa mesajı göster ve kullanıcının yazdığı verileri (dto) geri gönder
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        // 3. AJAX: Etkinlik Tipi Seçildiğinde Dinamik Alanları Getirir
        [HttpGet]
        public async Task<IActionResult> GetFieldsByEventType(int eventTypeId)
        {
            var fields = await _unitOfWork.Repository<Domain.Entities.EventField>()
              .FindAsync(f => f.EventTypeId == eventTypeId);

            return Json(fields.Select(f => new { f.Id, f.Name, f.DataType }));
        }
        private async Task LoadViewBagData()
        {
            // Veritabanından Salonları ve Etkinlik Tiplerini tekrar çekiyoruz
            ViewBag.Resources = await _unitOfWork.Repository<Domain.Entities.Resource>().GetAllAsync();
            ViewBag.EventTypes = await _unitOfWork.Repository<Domain.Entities.EventType>().GetAllAsync();
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

        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için ekledik
        public async Task<IActionResult> AddSeat(int resourceId, int eventTypeId, DateTime start, DateTime end, string seatNo)
        {
            if (start < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş tarihteki bir seansa ekleme yapılamaz.";
                return RedirectToAction(nameof(Index));
            }

            // Dinamik olarak 'Koltuk No' alanının ID'sini bulalım (ID 1 olmayabilir)
            var seatFields = await _unitOfWork.Repository<EventField>()
                .FindAsync(f => f.Name.Contains("Koltuk") && f.EventTypeId == eventTypeId);

            var seatField = seatFields.FirstOrDefault();

            if (seatField == null)
            {
                TempData["ErrorMessage"] = "Koltuk numarası alanı sistemde tanımlı değil.";
                return RedirectToAction(nameof(Index));
            }

            // Aynı koltuk o saatte dolmuş mu kontrolü
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
                    EventFieldId = seatField.Id, // Elle 1 yazmak yerine dinamik ID kullandık
                    Value = seatNo
                    }
                }
            };

            await _unitOfWork.Repository<Reservation>().AddAsync(newReservation);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Koltuk {seatNo} başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBySalon(int resourceId, DateTime startTime)
        {
            // 1. O salon ve o saatteki tüm kayıtları buluyoruz
            var reservationsToDelete = await _unitOfWork.Repository<Reservation>()
                .FindAsync(r => r.ResourceId == resourceId && r.StartTime == startTime);

            if (reservationsToDelete != null && reservationsToDelete.Any())
            {
                try
                {
                    // 2. Her bir rezervasyonu (ve bağlı EventValues'larını) siliyoruz
                    foreach (var res in reservationsToDelete)
                    {
                        _unitOfWork.Repository<Reservation>().Delete(res);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Salonun bu saatteki tüm rezervasyonları temizlendi.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Silme işlemi sırasında bir hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction("Index", "Home"); // Ana sayfaya yönlendir
        }
    }
}
    