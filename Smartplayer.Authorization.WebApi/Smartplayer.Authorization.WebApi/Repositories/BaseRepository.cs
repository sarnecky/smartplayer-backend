using Microsoft.EntityFrameworkCore;
using Smartplayer.Authorization.WebApi.Common;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories
{
    public abstract class BaseRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        public ApplicationDbContext _applicationDbContext;
        public readonly DbSet<TAggregate> _dbSet;
        public BaseRepository(ApplicationDbContext context)
        {
            _applicationDbContext = context;
            _dbSet = _applicationDbContext.Set<TAggregate>();
        }

        public async Task<TAggregate> AddAsync(TAggregate item)
        {
            var result = _dbSet.Add(item)?.Entity;
            if (result != null)
                await _applicationDbContext.SaveChangesAsync();
            return result;
        }

        public async Task<bool> Delete(TAggregate item)
        {
            bool result = (_dbSet.Remove(item)?.Entity != null);
            if (result != false)
                await _applicationDbContext.SaveChangesAsync();
            return result;
        }

        public async Task<TAggregate> FindById(int id)
        {
            var result = await _dbSet.AsQueryable().SingleOrDefaultAsync(i => i.Id == id).ConfigureAwait(false);
            return result;
        }

        public async Task<TAggregate> Update(TAggregate item)
        {
            var result = _dbSet.Update(item)?.Entity;
            if (result != null)
                await _applicationDbContext.SaveChangesAsync();
            return result;
        }

        public async Task<IList<TAggregate>> FindByCriteria(Expression<Func<TAggregate, bool>> criteria)
        {
            var result = await _dbSet.AsQueryable().Where(criteria).ToListAsync();
            return result;
        }

        public async Task<IList<TAggregate>> FindByCriteria<TProperty>(Expression<Func<TAggregate, bool>> criteria, Expression<Func<TAggregate, TProperty>> include)
        {
            var result = await _dbSet.AsQueryable().Include(include).Where(criteria).ToListAsync();
            return result;
        }

        public async Task<TAggregate> FindSingleOrDefaultByCriteria(Expression<Func<TAggregate, bool>> criteria)
        {
            var result = await _dbSet.AsQueryable().SingleOrDefaultAsync(criteria).ConfigureAwait(false);
            return result;
        }
    }
}
