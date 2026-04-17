using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Domain.Common
{
    //BaseEntity abstract yapmadık çünkü absract metodu yok.Belki nesne olarak kullanmak isteyebiliriz.
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } //Güncelleme yapılmayabilir bu yüzden null olabilir.
        public bool IsActive { get; set; } = true;
    }
}

/*BaseEntity temiz kod yazmamızı sağlar.Veri bütünlüğünü sağlar.Geliştirmeyi hızlandırır.
 Generic Repository desteği sağlar.*/