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
        public string Name { get; set; } = string.Empty; 
        // Tip ismi (Seminer,Konferans),başlangıçta hata vermemesi için boş string yaptım.
        public string? Description { get; set; } // Doldurulması zorunlu değildir.

        // Navigasyon özellikleri bu alana ait olan diğer varlıklarla ilişkiler kurar.Tembel yükleme için
        //virtual kullandık.
        public virtual ICollection<EventField> EventFields { get; set; } = new List<EventField>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
