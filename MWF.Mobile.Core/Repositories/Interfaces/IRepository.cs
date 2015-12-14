using System;
using SQLite.Net;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;
using SQLite.Net.Attributes;
using System.Threading.Tasks;
using SQLite.Net.Async;

namespace MWF.Mobile.Core.Repositories
{

    public interface IRepository<T> where T : IBlueSphereEntity, new()
    {
        Task DeleteAsync(T entity);
        Task DeleteAllAsync();

        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIDAsync(Guid ID);

        void Insert(T entity, SQLiteConnection connection);
        Task InsertAsync(T entity);
        void Insert(IEnumerable<T> entities, SQLiteConnection connection);
        Task InsertAsync(IEnumerable<T> entities);

        Task UpdateAsync(T entity);
    }
}
