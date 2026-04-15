using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EtkinlikSeminerKayit.WebApp.Controllers
{
    public class ResourceAdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ResourceAdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Salon Listesi
        public async Task<IActionResult> Index()
        {
            var resources = await _unitOfWork.Repository<Resource>().GetAllAsync();

            // Türleri çek (ViewBag ile gönderiyoruz)
            ViewBag.EventTypes = await _unitOfWork.Repository<EventType>().GetAllAsync();
            return View(resources);
        }

        // Kapasite Güncelleme (Hızlı Düzenleme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateResource(int id, string name, int capacity)
        {
            // Salonu bul
            var resource = await _unitOfWork.Repository<Resource>().GetByIdAsync(id);

            if (resource == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek salon bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Bilgileri güncelle
            resource.Name = name;
            resource.Capacity = capacity;
            resource.UpdatedDate = DateTime.Now;

            try
            {
                _unitOfWork.Repository<Resource>().Update(resource);
                await _unitOfWork.SaveChangesAsync();
                TempData["SuccessMessage"] = "Salon bilgileri güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Güncelleme sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        // Salon Ekleme Metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, int capacity)
        {
            if (string.IsNullOrWhiteSpace(name) || capacity <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir salon ismi ve kapasite giriniz.";
                return RedirectToAction(nameof(Index));
            }

            var newResource = new Resource
            {
                Name = name,
                Capacity = capacity,
                CreatedDate = DateTime.Now // Eğer base entity kullanıyorsan
            };

            await _unitOfWork.Repository<Resource>().AddAsync(newResource);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{name} başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }
        // --- SALON SİLME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _unitOfWork.Repository<Resource>().GetByIdAsync(id);
            if (resource != null)
            {
                // Önce bu salona ait rezervasyon var mı kontrol etmek iyi bir pratiktir
                var hasReservations = await _unitOfWork.Repository<Reservation>().AnyAsync(r => r.ResourceId == id);
                if (hasReservations)
                {
                    TempData["ErrorMessage"] = "Bu salona ait rezervasyonlar olduğu için silinemez.";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.Repository<Resource>().Delete(resource);
                await _unitOfWork.SaveChangesAsync();
                TempData["SuccessMessage"] = "Salon başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- ETKİNLİK TÜRÜ EKLEME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEventType(string typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                var newType = new EventType { Name = typeName };
                await _unitOfWork.Repository<EventType>().AddAsync(newType);
                await _unitOfWork.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{typeName}' türü başarıyla eklendi.";
            }
            return RedirectToAction(nameof(Index)); // Aynı sayfaya dön
        }

        // --- ETKİNLİK TÜRÜ SİLME ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            // 1. Silinmek istenen türü bul
            var eventType = await _unitOfWork.Repository<EventType>().GetByIdAsync(id);

            if (eventType != null)
            {
                // 2. Bu etkinlik tipine bağlı herhangi bir rezervasyon var mı kontrol et
                // (Bu tipte kayıt varsa silme işlemini engelle)
                var hasLinkedReservations = await _unitOfWork.Repository<Reservation>()
                    .AnyAsync(r => r.EventTypeId == id);

                if (hasLinkedReservations)
                {
                    TempData["ErrorMessage"] = $"'{eventType.Name}' türünde mevcut kayıtlar/rezervasyonlar olduğu için bu türü silemezsiniz.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    // 3. Eğer bağlı kayıt yoksa sil
                    _unitOfWork.Repository<EventType>().Delete(eventType);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Etkinlik türü başarıyla silindi.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Silme işlemi sırasında bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Silinmek istenen etkinlik türü bulunamadı.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
