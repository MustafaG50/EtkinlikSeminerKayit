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
        public int ResourceId { get; set; } // Hangi salon?
        public int EventTypeId { get; set; } // Hangi etkinlik tipi?
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Admin'in doldurduğu dinamik alanların listesi
        public List<DynamicFieldValueDto> DynamicValues { get; set; }

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
