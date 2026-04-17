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
        public string Name { get; set; } = string.Empty; //Salon Adı
        public int Capacity { get; set; } // Salon kapasitesi
        public string? Description { get; set; } // Salon hakkında ek bilgiler (isteğe bağlı)

        // Navigasyon özellikleri
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
