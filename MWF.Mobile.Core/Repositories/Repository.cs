using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Helpers;
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
                //Contract.Requires<ArgumentNullException>(dataService != null, "dataService cannot be null");

                _connection = dataService.Connection;
            }

            #endregion

            #region IRepository<T> Members

            public virtual void Insert(T entity)
            {
                //Contract.Requires<ArgumentNullException>(entity != null, "entity cannot be null");

                _connection.RunInTransaction(() =>
                 {     
                    InsertRecursive(entity);
                 });
            }

            public virtual void Insert(List<T> entities)
            {
                //Contract.Requires<ArgumentNullException>(entities != null, "entities cannot be null");

                _connection.RunInTransaction(() =>
                {
                    foreach (var entity in entities)
                    {
                        InsertRecursive(entity);
                    }

                });

            }

            public virtual void Delete(T entity)
            {
                //Contract.Requires<ArgumentNullException>(entity != null, "entity cannot be null");

                _connection.RunInTransaction(() =>
                {
                    DeleteRecursive(entity);
                });
            }


            public virtual IEnumerable<T> GetWhere(Expression<Func<T, bool>> predicate)
            {
                var entities = _connection.Table<T>().Where(predicate);

                if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entities);

                return entities;
            }

            public virtual IEnumerable<T> GetAll()
            {
                var entities = _connection.Table<T>();

                if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entities);

                return entities;
            }

            public virtual T GetByID(Guid ID)
            {
                T entity = _connection.Table<T>().Single(e => e.ID == ID);

                if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entity);

                return entity;

            }

            #endregion

            #region Private Methods


            private void InsertRecursive(IBlueSphereEntity entity)
            {
                _connection.Insert(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    IEnumerable<IBlueSphereEntity> children = relationshipProperty.GetValue(entity, null) as IEnumerable<IBlueSphereEntity>;
                    if (children == null)
                    {
                        throw new ArgumentException(string.Format("{0} type property {1} does not contain BlueSphere entities", entity.GetType().ToString(), relationshipProperty.Name));
                    }

                    foreach (var child in children)
                    {
                        InsertRecursive(child);
                    }

                }
            }

            private void DeleteRecursive(IBlueSphereEntity entity)
            {
                _connection.Delete(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    IEnumerable<IBlueSphereEntity> children = relationshipProperty.GetValue(entity, null) as IEnumerable<IBlueSphereEntity>;
                    if (children == null)
                    {
                        throw new ArgumentException(string.Format("{0} type property {1} does not contain BlueSphere entities", entity.GetType().ToString(), relationshipProperty.Name));
                    }

                    foreach (var child in children)
                    {
                        DeleteRecursive(child);
                    }

                }
            }

            private void PopulateChildrenRecursive(IEnumerable parents)
            {
                foreach (var parent in parents)
                {
                    PopulateChildrenRecursive(parent as IBlueSphereEntity);              
                }
            }

            private void PopulateChildrenRecursive(IBlueSphereEntity parent)
            {

                foreach (var relationshipProperty in parent.GetType().GetChildRelationProperties())
                {
                    Type childType = relationshipProperty.GetTypeOfChildRelation();
                    IList children = GetChildren(parent, childType);
                    relationshipProperty.SetValue(parent, children);
                    PopulateChildrenRecursive(children);

                }
            }

            // Gets the children of the specfied type for specified parent using foreign key mappings
            private IList GetChildren(IBlueSphereEntity parent, Type childType)
            {
                ITableMapping tableMapping = _connection.GetMapping(childType);

                string query = string.Format("select * from {0} where {1} = ?", childType.GetTableName(),
                                                                                childType.GetForeignKeyName(parent.GetType()));
                   
                List<object> queryResults = _connection.Query(tableMapping, query, parent.ID);

                // Create a typed generic list we can assign back to parent element
                IList genericList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

                foreach (object item in queryResults)
                {
                    genericList.Add(item);
                }

                return genericList;

            }



            #endregion


        }

}
