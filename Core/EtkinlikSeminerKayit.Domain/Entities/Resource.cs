using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Entities
{
    public class Resource : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; } // Örn: 20
        public string? Description { get; set; }

        // Navigation Property: Bu odadaki rezervasyonlar
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
