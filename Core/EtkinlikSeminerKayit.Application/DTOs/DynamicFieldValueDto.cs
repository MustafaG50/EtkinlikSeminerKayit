using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.DTOs
{
    public class DynamicFieldValueDto
    {
        public int EventFieldId { get; set; } // Hangi alan? (Örn: Koltuk No ID'si)
        public string Value { get; set; } = string.Empty; // Girilen değer (Örn: "21")
    }
}
