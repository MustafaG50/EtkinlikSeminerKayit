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
        public string Value { get; set; } = string.Empty; // Her şey string tutulur

        public int EventFieldId { get; set; }
        public virtual EventField EventField { get; set; } = null!;

        public int ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; } = null!;
    }
}
