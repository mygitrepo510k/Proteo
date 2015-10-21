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
        void Delete(T entity, SQLiteConnection connection);
        void Delete(T entity);
        void DeleteAll(SQLiteConnection connection);
        void DeleteAll();
        Task DeleteAllAsync();
        IEnumerable<T> GetAll(SQLiteConnection connection);
        IEnumerable<T> GetAll();
        T GetByID(Guid ID,SQLiteConnection connection);
        T GetByID(Guid ID);
        void Insert(T entity, SQLiteConnection connection);
        void Insert(T entity);

        Task InsertAsync(T entity, SQLiteAsyncConnection connection);
        Task InsertAsync(T entity);
        void Insert(IEnumerable<T> entities, SQLiteConnection connection);
        void Insert(IEnumerable<T> entities);
        Task InsertAsync(IEnumerable<T> entities);
        void Update(T entity, SQLiteConnection connection);
        void Update(T entity);
    }
}
