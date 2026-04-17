using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Common;
using EtkinlikSeminerKayit.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        // İhtiyacımız olan repositoryi oluşturup dönderir.
        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            return new GenericRepository<T>(_context);
        }
        //Bu kod çalışana kadar db ye bilgiler kaydedilmez.
        //eğer hata çıkarsa hiçbir bilgi kaydedilmez.
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        //DB bağlantısını kapatır ve belleği temizler.
        public void Dispose() => _context.Dispose();
    }
}
