using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Repositories
{
    public interface IRepository<T> where T : IBlueSphereEntity, new()
    {
        void Delete(T entity);
        IEnumerable<T> GetAll();
        T GetByID(Guid ID);
        IEnumerable<T> GetWhere(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        void Insert(T entity);
        void Insert(List<T> entities);
    }
}
