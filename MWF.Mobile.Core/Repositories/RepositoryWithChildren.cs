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

    public abstract class RepositoryWithChidren<T1,T2> : Repository<T1>  
                                                        where T1 : IBlueSphereParentEntity<T2>, new()
                                                        where T2 : IBlueSphereChildEntity, new() 
    {


        #region Construction

        public RepositoryWithChidren(IDataService dataService) : base(dataService)
        {
        }

        #endregion

        #region IRepository<T> Members

        public override void Insert(T1 entity)
        {
            _connection.RunInTransaction(() =>
            {
                _connection.Insert(entity);
                foreach (var child in entity.Children)
                {
                    _connection.Insert(child); 
                }

            });

        }

        public override void Insert(List<T1> entities)
        {
            _connection.RunInTransaction(() =>
            {
                foreach (var entity in entities)
                {
                    Insert(entity);
                }

            });

        }


        public override void Delete(T1 entity)
        {
            _connection.RunInTransaction(() =>
            {
                _connection.Delete(entity);
                foreach (var child in entity.Children)
                {
                    _connection.Delete(child);
                }

            });

        }

        public override IEnumerable<T1> GetWhere(Expression<Func<T1, bool>> predicate)
        {
            var parents = _connection.Table<T1>().Where(predicate);

            PopulateChildren(parents);

            return parents;
        }


        public override IEnumerable<T1> GetAll()
        {
            var parents = _connection.Table<T1>();

            PopulateChildren(parents);

            return parents;

        }

        public override T1 GetByID(Guid ID)
        {
            var parent = _connection.Table<T1>().Single(e => e.ID == ID);

            PopulateChildren(new List<T1>() { parent });

            return parent;
        }

        #endregion

        #region  Methods

        // Given a list of parents, populates each parents "children" collection using the 
        // the child table associated with it
        protected abstract void PopulateChildren(IEnumerable<T1> parents);

        #endregion

    }

}
