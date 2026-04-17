using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Application.Interfaces
{
    //DB ye yarım veri yazmamızı engeller.Bütünlük Sağlar.
    //Bir tabloda hata olurs diğerlerinide yazmaz.
    //IDisposable db bağlantısını kapatmak için kullanılır.
    public interface IUnitOfWork :IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        //Her bir tablo için ayrı repository yazmak generic repositoryden tüm tabloları yönetebiliriz.
        Task<int> SaveChangesAsync(); // Repositorylerde yapılan işlemleri DB ye kaydetmek için kullanılır.
    }
}
