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
        Task DeleteAsync(T entity, SQLiteAsyncConnection connection);
        Task DeleteAsync(T entity);
        Task DeleteAllAsync(SQLiteAsyncConnection connection);
        Task DeleteAllAsync();

        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(SQLiteAsyncConnection connection);

        Task<T> GetByIDAsync(Guid ID);
        Task<T> GetByIDAsync(Guid ID,SQLiteAsyncConnection connection);

        Task InsertAsync(T entity, SQLiteAsyncConnection connection);
        Task InsertAsync(T entity);
        Task InsertAsync(IEnumerable<T> entities, SQLiteAsyncConnection connection);
        Task InsertAsync(IEnumerable<T> entities);

        Task UpdateAsync(T entity, SQLiteAsyncConnection connection);
        Task UpdateAsync(T entity);
    }
}
