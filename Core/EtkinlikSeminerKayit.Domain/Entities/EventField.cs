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
        public string Name { get; set; } = string.Empty; // Dinamik Bilginin adı(Koltuk no)
        public FieldDataType DataType { get; set; }      // Ne tür veri girileceğini belirler.(Text, Number, DateTime,Boolean)
        public bool IsRequired { get; set; } // Alanın boş kalıp kalmayacağını belirler.True ise doldurmadan geçemez
        public int EventTypeId { get; set; } // Hangi etkinlik türüne ait olduğunu belirtir. Foreign Key olur.
        public virtual EventType EventType { get; set; } = null!; //Başlangıçta null ama çalışınca dolacak.
        /* EventType ile ilişkiyi oluşturur . Bir EventFieldin bir etkinlik türüne ait olduğunu belirtir. 
         Navigation property olarak kullanılır. İki tabloyu ilişkilendirir. virtual anahtar kelimesi tembel
         yükleme(İhtiyaç olduğunda db den bilgiler gelir) için kullanılır.*/
    }
}
