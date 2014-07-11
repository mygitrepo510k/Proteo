using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.Repositories
{
    
        public abstract class Repository<T> : IRepository<T> where T : IBlueSphereEntity, new()
        {

            protected ISQLiteConnection _connection;

            #region Construction

            public Repository(IDataService dataService)
            {
                _connection = dataService.Connection;
            }

            #endregion

            #region IRepository<T> Members

            public virtual void Insert(T entity)
            {
                _connection.Insert(entity);
            }

            public virtual void Insert(List<T> entities)
            {
                _connection.RunInTransaction(() =>
                {
                    foreach (var entity in entities)
                    {
                        Insert(entity);
                    }

                });

            }

            public virtual void Delete(T entity)
            {
                _connection.Delete(entity);
            }

            public virtual IEnumerable<T> GetWhere(Expression<Func<T, bool>> predicate)
            {
                return _connection.Table<T>().Where(predicate);
            }

            public virtual IEnumerable<T> GetAll()
            {
                return _connection.Table<T>();
            }

            public virtual T GetByID(Guid ID)
            {
                return _connection.Table<T>().Single(e => e.ID == ID);
            }

            #endregion
        }

}
