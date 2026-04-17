using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.DTOs
{
    public class DynamicFieldValueDto
    {
        public int EventFieldId { get; set; } // Hangi alan koltuk no gibi
        public string Value { get; set; } = string.Empty; // Girilen değer 10 gibi
    }
}
