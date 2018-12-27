using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories.Interfaces
{
    public interface IRepository<TAggregate>
    {
        Task<TAggregate> AddAsync(TAggregate item);
        Task<TAggregate> Update(TAggregate item);
        Task<bool> Delete(TAggregate item);
        Task<TAggregate> FindById(int id);
        Task<IList<TAggregate>> FindByCriteria(Expression<Func<TAggregate, bool>> criteria);
        Task<IList<TAggregate>> FindByCriteria<TProperty>(Expression<Func<TAggregate, bool>> criteria, Expression<Func<TAggregate, TProperty>> include);
        Task<TAggregate> FindSingleOrDefaultByCriteria(Expression<Func<TAggregate, bool>> criteria);
    }
}
