using EtkinlikSeminerKayit.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace EtkinlikSeminerKayit.Application.Interfaces
{
    //Her tablo için ayrı ayrı kod yazmamıza gerek kalmaz.
    //Ve sadece BaseEntity sınıfından türeyen sınıflar için geçerli olur.
    //Veritabanı bağımsız şekilde çalışır.
    //Task Kullanarak asenkron çalışır.
    //DB işlemleri zaman alır, asenkron olarak çalışmak performansı artırır.
    public interface IGenericRepository<T> where T : BaseEntity
    {
        //Verilen Id ye göre 1 kayıt getirir.
        Task<T?> GetByIdAsync(int id);
        //DB deki tüm kayıtları getirir. İstersek belli şarta göre filtreleme yapabiliriz.
        //İstersek ilişkili tabloları da dahil edebiliriz. Navigasyon özelliği
        Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        //Bir Şarta göre arama yapar.
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        //DB ye Yeni bir kayıt ekler. 
        Task AddAsync(T entity);
        //kaydı günceller.
        void Update(T entity);
        //kaydı siler.
        void Delete(T entity);
        //Belli bir şarta uyan kaç kayıt olduğunu sayar.
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        //Belli bir şarta uyan kayıt varmı diye bakar.
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
    /*async: Bir metodun içinde asenkron işlemler yapılacağını belirtir.
    await: "Bu Task bitene kadar burada bekle ama uygulamayı dondurma,
    işlem bitince bir sonraki satıra geç" demektir.*/
}
