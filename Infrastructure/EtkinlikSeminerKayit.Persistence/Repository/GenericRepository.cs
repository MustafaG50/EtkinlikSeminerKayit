using EtkinlikSeminerKayit.Application.Interfaces;
using EtkinlikSeminerKayit.Domain.Common;
using EtkinlikSeminerKayit.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EtkinlikSeminerKayit.Persistence.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>(); //T yerine hangi entity gelirse onun DbSet'ini alırız
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);// Filtre yapılan yer
            }

            if (include != null)
            {
                query = include(query); // Include işlemi yapılan yer
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity); 
        // async değil çünkü update işlemi entity'nin durumunu değiştirir, db ye kaydetme işlemi SaveChangesAsync() ile yapılır

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.CountAsync(predicate);
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
