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

        // Bağımlılıkları ekledik.
        public ReservationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsSuccess, string Message)> CreateReservationAsync(CreateReservationDto dto)
        {
            try
            {
                // Kaynak Kontrolü yapar.
                var resource = await _unitOfWork.Repository<Resource>().GetByIdAsync(dto.ResourceId);
                if (resource == null)
                {
                    return (false, "Belirtilen salon veya kaynak bulunamadı.");
                }
                //Geçmiş tarih kontrolü.
                if (dto.StartTime < DateTime.Now)
                {
                    return (false, "Geçmiş bir tarihe veya saate rezervasyon yapılamaz.");
                }
                // Çakışma Kontrolü yapar.
                var overlappingReservations = await _unitOfWork.Repository<Reservation>()
                    .FindAsync(r => r.ResourceId == dto.ResourceId &&
                    dto.StartTime < r.EndTime &&
                    dto.EndTime > r.StartTime);
                // Koltuk seçimi yaparken çakışma kontrolünü es geçer.
                bool isKoltukSecimiVar = dto.DynamicValues != null && dto.DynamicValues.Any(v => v.Value != null);

                if (!isKoltukSecimiVar && overlappingReservations.Any())
                {
                    return (false, "Bu salon seçilen saatler arasında zaten rezerve edilmiş.");
                }

                // Kapasite aşımını engellemek için mevcut rezervasyon sayısını kontrol eder.
                if (overlappingReservations.Count() >= resource.Capacity)
                {
                    return (false, $"Kapasite Aşımı! {resource.Name} kontenjanı dolmuştur. (Kapasite: {resource.Capacity})");
                }
                // Koltuk seçimi yapılırken seçilen koltuğun dolu olup olmadığını kontrol eder.
                if (dto.DynamicValues != null && dto.DynamicValues.Any())
                {
                    foreach (var dynamicVal in dto.DynamicValues)
                    {
                        var isValueAlreadyTaken = await _unitOfWork.Repository<EventValue>().AnyAsync(ev =>
                            ev.EventFieldId == dynamicVal.EventFieldId && // Aynı alan 
                            ev.Value == dynamicVal.Value &&               // Aynı değer 
                            ev.Reservation.ResourceId == dto.ResourceId && // Aynı salon
                            dto.StartTime < ev.Reservation.EndTime &&     // Çakışan zaman dilimi
                            dto.EndTime > ev.Reservation.StartTime);

                        if (isValueAlreadyTaken)
                        {
                            return (false, $"Hata: '{dynamicVal.Value}' numaralı koltuk/alan bu saatler için zaten rezerve edilmiş!");
                        }
                    }
                }
                // Kapasite uygunsa, çakışma yoksa yeni rezervasyon oluşturur.
                var newReservation = new Reservation
                {
                    ResourceId = dto.ResourceId,
                    EventTypeId = dto.EventTypeId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    EventValues = new List<EventValue>() // Değerleri eklemek için boş liste oluşturuldu.
                };

                // Dinamik değerleri EventValue olarak ekler.Reservation nesnesine ekler.
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

                // Rezervasyonu DB ye ekler.
                await _unitOfWork.Repository<Reservation>().AddAsync(newReservation);

                // Unitofwork ile tüm işlemleri kaydet.hata olursa catch bloğuna gider.
                var result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return (true, "Rezervasyon başarıyla oluşturuldu.");
                }

                return (false, "Kayıt işlemi sırasında bir hata oluştu.");
            }
            catch (Exception ex)
            {
                // Hata mesajı gönderir.
                return (false, $"Sistemsel bir hata oluştu: {ex.Message}");
            }
        }
    }
}
