using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Entities
{
    public class EventType : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: "Seminer", "Laboratuvar Dersi"
        public string? Description { get; set; }

        // Navigation Properties: Bu etkinlik tipine ait alanlar ve rezervasyonlar
        public virtual ICollection<EventField> EventFields { get; set; } = new List<EventField>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
