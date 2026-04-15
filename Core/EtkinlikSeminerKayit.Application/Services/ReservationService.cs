using EtkinlikSeminerKayit.Application.DTOs;
using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Dependency Injection (Bağımlılıkların Enjekte Edilmesi)
        public ReservationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsSuccess, string Message)> CreateReservationAsync(CreateReservationDto dto)
        {
            try
            {
                // 1. Kaynağı (Oda/Salon) bul ve kapasitesini al
                var resource = await _unitOfWork.Repository<Resource>().GetByIdAsync(dto.ResourceId);
                if (resource == null)
                {
                    return (false, "Belirtilen salon veya kaynak bulunamadı.");
                }

                // 2. Çakışma ve Mevcut Doluluk Kontrolü Algoritması
                // Kural: (Yeni Başlangıç < Mevcut Bitiş) VE (Yeni Bitiş > Mevcut Başlangıç)
                var overlappingReservations = await _unitOfWork.Repository<Reservation>()
                    .FindAsync(r => r.ResourceId == dto.ResourceId &&
                    dto.StartTime < r.EndTime &&
                    dto.EndTime > r.StartTime);
                // --- YENİ MANTIK: Salon bazlı tam çakışma kontrolü ---
                // Eğer bu bir "Salon Rezervasyonu" ise (Koltuk seçilmeden yapılıyorsa) 
                // ve içeride zaten başka bir kayıt varsa çakışma ver.
                bool isKoltukSecimiVar = dto.DynamicValues != null && dto.DynamicValues.Any(v => v.Value != null);

                if (!isKoltukSecimiVar && overlappingReservations.Any())
                {
                    return (false, "Bu salon seçilen saatler arasında zaten rezerve edilmiş.");
                }

                // 3. Dinamik Kapasite Kuralı
                // Eğer o saatteki kayıt sayısı, salonun güncel kapasitesine eşit veya büyükse reddet
                // 3. Mevcut Kapasite Kuralı (Kişi bazlı eklemeler için)
                if (overlappingReservations.Count() >= resource.Capacity)
                {
                    return (false, $"Kapasite Aşımı! {resource.Name} kontenjanı dolmuştur. (Kapasite: {resource.Capacity})");
                }
                // --- YENİ: 3. DİNAMİK ALAN (KOLTUK NO VB.) ÇAKIŞMA KONTROLÜ ---
                if (dto.DynamicValues != null && dto.DynamicValues.Any())
                {
                    foreach (var dynamicVal in dto.DynamicValues)
                    {
                        // KURAL: Aynı salon, aynı zaman dilimi ve aynı alanda (FieldId) aynı değer (Value) var mı?
                        var isValueAlreadyTaken = await _unitOfWork.Repository<EventValue>().AnyAsync(ev =>
                            ev.EventFieldId == dynamicVal.EventFieldId && // Aynı alan (örn: Koltuk No)
                            ev.Value == dynamicVal.Value &&               // Aynı değer (örn: "15")
                            ev.Reservation.ResourceId == dto.ResourceId && // Aynı salon
                            dto.StartTime < ev.Reservation.EndTime &&     // Çakışan zaman dilimi
                            dto.EndTime > ev.Reservation.StartTime);

                        if (isValueAlreadyTaken)
                        {
                            return (false, $"Hata: '{dynamicVal.Value}' numaralı koltuk/alan bu saatler için zaten rezerve edilmiş!");
                        }
                    }
                }
                // -----------------------------------------------------------
                // 4. Kapasite uygunsa Ana Rezervasyon Kaydını Oluştur
                var newReservation = new Reservation
                {
                    ResourceId = dto.ResourceId,
                    EventTypeId = dto.EventTypeId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    EventValues = new List<EventValue>() // EAV değerlerini eklemek için listeyi başlatıyoruz
                };

                // 5. Dinamik Alanları (EAV) Ana Kayda Bağla
                // Admin panelinden gönderilen "Koltuk No: 21", "TC: 123" gibi verileri eşleştiriyoruz
                if (dto.DynamicValues != null && dto.DynamicValues.Any())
                {
                    foreach (var dynamicVal in dto.DynamicValues)
                    {
                        newReservation.EventValues.Add(new EventValue
                        {
                            EventFieldId = dynamicVal.EventFieldId,
                            Value = dynamicVal.Value
                        });
                    }
                }

                // 6. Veritabanına Ekleme İşlemi (Henüz Commit edilmedi)
                await _unitOfWork.Repository<Reservation>().AddAsync(newReservation);

                // 7. UnitOfWork ile İşlemi Tamamla (Transaction)
                // Eğer EventValues tablosuna kayıt sırasında hata çıkarsa, Reservation da iptal olur.
                var result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return (true, "Rezervasyon başarıyla oluşturuldu.");
                }

                return (false, "Kayıt işlemi sırasında bir hata oluştu.");
            }
            catch (Exception ex)
            {
                // Loglama mekanizması eklendiğinde ex.Message loglanabilir
                return (false, $"Sistemsel bir hata oluştu: {ex.Message}");
            }
        }
    }
}
