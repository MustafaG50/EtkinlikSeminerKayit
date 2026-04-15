using EtkinlikSeminerKayit.Domain.Common;
using EtkinlikSeminerKayit.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Entities
{
    public class EventField : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: "Koltuk No"
        public FieldDataType DataType { get; set; }      // Örn: Number
        public bool IsRequired { get; set; }

        public int EventTypeId { get; set; }
        public virtual EventType EventType { get; set; } = null!;
    }
}
