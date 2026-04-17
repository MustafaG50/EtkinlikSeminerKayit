using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Entities
{
    public class Reservation : BaseEntity
    {
        public DateTime StartTime { get; set; } // Başlangıç zamanı
        public DateTime EndTime { get; set; } // Bitiş zamanı

        public int ResourceId { get; set; }  //Rezervasyob kaynağı
        public virtual Resource Resource { get; set; } = null!;

        public int EventTypeId { get; set; } // Rezervasyonun etkinlik tipi
        public virtual EventType EventType { get; set; } = null!;

        // Entity - Attribute - Value ilişkisi için
        public virtual ICollection<EventValue> EventValues { get; set; } = new List<EventValue>();
    }
}
