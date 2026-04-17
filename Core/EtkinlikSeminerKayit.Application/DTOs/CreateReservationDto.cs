using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.DTOs
{
    public class CreateReservationDto : IValidatableObject 
    {
        //Kayıt oluştururken gereken veriler kullanılır.Entitylerden çok daha sadedir
        public int ResourceId { get; set; } // Hangi salonu seçtiğini gösterir
        public int EventTypeId { get; set; } // Hangi etkinlik tipini seçtiğini gösterir
        public DateTime StartTime { get; set; } // Rezervasyonun başlangıç zamanı
        public DateTime EndTime { get; set; } // Rezervasyonun bitiş zamanı

        // Kullanıcının doldurduğu dinamik alanların listesi
        public List<DynamicFieldValueDto> DynamicValues { get; set; }
        // List kullandık çünkü dinamik alanlar değişebilir.

        // IValidatableObject arayüzü ile gelen verilarin mantıklı olup olmadığına bakarız.
        // Validate içinde yazdıklarımız otomatik olarak çalışır hata varsa ValidationResult döner.
        // ve hatanın ne olduğunu belirtiriz.yield return ile hatalar birikir ve hepsi döner.Tek hatada durmaz
        // diğer hataları da kontrol eder.ValidationResult hata mesajı ve hata yeri değerlerini alır.
        // kullanıcıya neyin yanlış olduğunu gösterebiliriz.
        // Dizi olarak hata yerlerini veririz birden fazla hata olabilir.
        // hem bitiş zamanı yanlış hem de başlangıç zamanı geçmiş olabilir.
        // Yanlış ve mantıksız verilerin db ye gitmesini engelleriz.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "Bitiş zamanı başlangıç zamanından sonra olmalıdır.",
                    new[] { nameof(EndTime) });
            }

            if (StartTime < DateTime.Now)
            {
                yield return new ValidationResult(
                    "Geçmiş bir tarihe rezervasyon yapılamaz.",
                    new[] { nameof(StartTime) });
            }
        }
    }
}
