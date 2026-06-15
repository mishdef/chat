using Microsoft.EntityFrameworkCore;
using SignalRProject.Data;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll() => _dbSet.ToList();

        public virtual T? GetById(int id)
        {
            var entity = _dbSet.Find(id);
            if (entity == null)
                throw new KeyNotFoundException("Entity not found.");
            return entity;
        }

        public virtual void Create(T item) { 
            _dbSet.Add(item);
            _context.SaveChanges();
        }

        public virtual void Update(T item)
        {
            _dbSet.Update(item);
            _context.SaveChanges();
        }

        public virtual void Delete(int id)
        {
            T? entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
            else
                throw new KeyNotFoundException("Entity not found.");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task CreateAsync(T item)
        {
            await _dbSet.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T item)
        {
            _dbSet.Update(item);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
            else
                throw new KeyNotFoundException("Entity not found.");
        }
    }
}