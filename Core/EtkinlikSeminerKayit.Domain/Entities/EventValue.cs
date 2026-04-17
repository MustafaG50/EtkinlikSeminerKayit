using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Entities
{
    public class EventValue : BaseEntity
    {
        public string Value { get; set; } = string.Empty;
        // Değerler string olarak tanımladık esnek olması için (FieldDataType araştır.)

        public int EventFieldId { get; set; } // Bu sadece veritabanında tutulan bir ID
        public virtual EventField EventField { get; set; } = null!; 
        //İlişkiyi yönetmek için kullanılan navigasyon özelliği.

        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; } = null!;
    }
}
