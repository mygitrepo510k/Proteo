using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Repositories
{

    public interface IRepository<T> where T : IBlueSphereEntity, new()
    {
        void Delete(T entity);
        void DeleteAll();
        IEnumerable<T> GetAll();
        T GetByID(Guid ID);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void Update(T entity);
    }
}
