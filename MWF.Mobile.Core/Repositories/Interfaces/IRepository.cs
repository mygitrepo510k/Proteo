using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace MWF.Mobile.Core.Repositories
{

    public interface IRepository<T> where T : IBlueSphereEntity, new()
    {
        void Delete(T entity, ISQLiteConnection connection);
        void Delete(T entity);
        void DeleteAll(ISQLiteConnection connection);
        void DeleteAll();
        IEnumerable<T> GetAll(ISQLiteConnection connection);
        IEnumerable<T> GetAll();
        T GetByID(Guid ID,ISQLiteConnection connection);
        T GetByID(Guid ID);
        void Insert(T entity, ISQLiteConnection connection);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities, ISQLiteConnection connection);
        void Insert(IEnumerable<T> entities);
        void Update(T entity, ISQLiteConnection connection);
        void Update(T entity);
    }
}
