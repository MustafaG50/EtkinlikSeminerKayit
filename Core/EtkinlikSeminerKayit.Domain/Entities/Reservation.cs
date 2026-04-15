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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int ResourceId { get; set; }
        public virtual Resource Resource { get; set; } = null!;

        public int EventTypeId { get; set; }
        public virtual EventType EventType { get; set; } = null!;

        // EAV Değerleri
        public virtual ICollection<EventValue> EventValues { get; set; } = new List<EventValue>();
    }
}
