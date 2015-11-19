using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Attributes;
using MWF.Mobile.Core.Services;
using SQLite.Net;
using System.Threading.Tasks;
using SQLite.Net.Async;

namespace MWF.Mobile.Core.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class, IBlueSphereEntity, new()
    {

        protected IDataService _dataService;


        #region Construction

        public Repository(IDataService dataService)
        {
            _dataService = dataService;
        }

        #endregion

        #region IRepository<T> Members

        public async virtual Task DeleteAllAsync(SQLiteAsyncConnection transactionConnection)
        {
            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();
            await _dataService.RunInTransactionAsync(async c =>
            {
                await DeleteAllRecursiveAsync(typeof(T), c);
            });
        }

        public async virtual Task DeleteAllAsync()
        {
            var connection = _dataService.GetAsyncDBConnection();
            await DeleteAllRecursiveAsync(typeof(T), connection);
        }

        public async virtual Task DeleteAsync(T entity, SQLiteAsyncConnection transactionConnection)
        {
            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();


            await _dataService.RunInTransactionAsync(async c =>
            {
                await DeleteRecursiveAsync(entity, c);
            });
        }

        public async virtual Task DeleteAsync(T entity)
        {
            await DeleteAsync(entity, null);
        }


        public async virtual Task<IEnumerable<T>> GetAllAsync(SQLiteAsyncConnection transactionConnection)
        {

            List<T> entities = null;

            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();
            entities = await connection.Table<T>().ToListAsync();
            if (typeof(T).HasChildRelationProperties()) await PopulateChildrenRecursive(entities, connection);

            return entities;
        }

        public async virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return await GetAllAsync(null);
        }


        public async virtual Task<T> GetByIDAsync(Guid ID, SQLiteAsyncConnection transactionConnection)
        {
            T entity = null;

            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();

            entity = connection.Table<T>().Where(e => e.ID == ID).FirstAsync().Result;

            if (entity != null)
                if (typeof(T).HasChildRelationProperties()) await PopulateChildrenRecursive(entity, connection);

            return entity;
        }



        public async virtual Task<T> GetByIDAsync(Guid ID)
        {
            return await GetByIDAsync(ID, null);
        }
        public async virtual Task InsertAsync(IEnumerable<T> entities, SQLiteAsyncConnection transactionConnection)
        {

            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();

            foreach (var entity in entities)
            {
                await InsertRecursiveAsync(entity, connection);
            }


        }

        public async virtual Task InsertAsync(T entity, SQLiteAsyncConnection transactionConnection)
        {

            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();
            await InsertRecursiveAsync(entity, connection);
        }

        public async virtual Task InsertAsync(IEnumerable<T> entities)
        {
            await InsertAsync(entities, null);
        }

        public async virtual Task InsertAsync(T entity)
        {
            await InsertAsync(entity, null);
        }


        private async Task InsertRecursiveAsync(IBlueSphereEntity entity, SQLiteAsyncConnection connection)
        {

            await connection.InsertAsync(entity);

            foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
            {
                DoRecursiveTreeAction(entity, relationshipProperty, async (parent, child) =>
                {
                    SetForeignKeyOnChild(parent, child);
                    await InsertRecursiveAsync(child, connection);
                });
            }
        }

        public async virtual Task UpdateAsync(T entity, SQLiteAsyncConnection transactionConnection)
        {

            SQLiteAsyncConnection connection = transactionConnection ?? _dataService.GetAsyncDBConnection();
            await _dataService.RunInTransactionAsync(async c =>
            {
                var existingEntity = await GetByIDAsync(entity.ID);
                if (existingEntity != null)
                {
                    await DeleteAsync(existingEntity);
                }

                await InsertRecursiveAsync(entity, c);
            });


        }

        public async virtual Task UpdateAsync(T entity)
        {
            await UpdateAsync(entity, null);
        }


        #endregion

        #region Private Methods





        /// <summary>
        ///  Deletes a potentially nested object graph from the appropriate tables using the
        ///  ChildRelationship and ForeignKey attributes to guide the process
        /// </summary>
        /// <param name="entity"></param>
        private async Task DeleteRecursiveAsync(IBlueSphereEntity entity, SQLiteAsyncConnection connection)
        {
            await connection.DeleteAsync(entity);

            foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
            {
                DoRecursiveTreeAction(entity, relationshipProperty, async (parent, child) =>
                {
                    await DeleteRecursiveAsync(child, connection);
                });

            }

        }

        /// <summary>
        /// Deletes all items of the specified type from their respective tables
        /// plus any child types as specified by the ChildRelationship attribute
        /// </summary>
        /// <param name="type"></param>
        private void DeleteAllRecursive(Type type, SQLiteConnection connection)
        {
            DeleteAllFromTable(type, connection);

            foreach (var childType in type.GetChildRelationTypes())
            {

                DeleteAllRecursive(childType, connection);

            }
        }

        private async Task DeleteAllRecursiveAsync(Type type, SQLiteAsyncConnection connection)
        {
            await DeleteAllFromTableAsync(type, connection);

            foreach (var childType in type.GetChildRelationTypes())
            {

                await DeleteAllRecursiveAsync(childType, connection);

            }
        }


        /// <summary>
        /// For the parent entity pulled from a table, populates any children
        /// as labelled with the ChildRelationship. 
        /// </summary>
        /// <param name="parent"></param>
        protected async Task PopulateChildrenRecursive(IBlueSphereEntity parent, SQLiteAsyncConnection connection)
        {

            foreach (var relationshipProperty in parent.GetType().GetChildRelationProperties())
            {
                Type childType = relationshipProperty.GetTypeOfChildRelation();
                string childIdentifyingPropertyName = relationshipProperty.GetIdentifyingPropertyNameOfChildRelation();
                object childIdentifyingPropertyValue = relationshipProperty.GetIdentifyingPropertyValueOfChildRelation();


                IList children = await GetChildren(parent, childIdentifyingPropertyName, childIdentifyingPropertyValue, connection);

                if (relationshipProperty.GetCardinalityOfChildRelation() == RelationshipCardinality.OneToOne)
                {

                    Debug.Assert(children.Count == 1);
                    relationshipProperty.SetValue(parent, children[0]);
                }
                else if (relationshipProperty.GetCardinalityOfChildRelation() == RelationshipCardinality.OneToZeroOrOne)
                {
                    Debug.Assert(children.Count <= 1);

                    if (children.Count == 1)
                        relationshipProperty.SetValue(parent, children[0]);
                }
                else
                {
                    relationshipProperty.SetValue(parent, children);
                }

                await PopulateChildrenRecursive(children, connection);

            }
        }



        /// <summary>
        /// Overload of PopulateChildrenRecursive that deals with
        /// a collection of parents
        /// </summary>
        /// <param name="parents"></param>
        protected async Task PopulateChildrenRecursive(IEnumerable parents, SQLiteAsyncConnection connection)
        {
            foreach (var parent in parents)
            {
                await PopulateChildrenRecursive(parent as IBlueSphereEntity, connection);
            }
        }


        // Gets the children of the specfied type for specified parent using foreign key mappings
        //private async Task<IList> GetChildren(IBlueSphereEntity parent, dynamic childType, string childIdentifyingPropertyName, object childIdentifyingPropertyValue, SQLiteAsyncConnection connection)
        //{
        //   // var t = Activator.CreateInstance<childType>();
        //    //TableMapping tableMapping =await connection.GetMappingAsync();

        //    string query = string.Format("select * from {0} where {1} = ?", childType.GetTableName(),
        //                                                                    childType.GetForeignKeyName(parent.GetType()));

        //    if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
        //    {
        //        query = query + string.Format(" AND {0} = ?", childIdentifyingPropertyName);
        //    }
        //    var listType = typeof(List<>);
        //    var constructedListType = listType.MakeGenericType(childType);
        //    var queryResults = Activator.CreateInstance(constructedListType);
        //    var t = Activator.CreateInstance(childType);


        //    if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
        //    {
        //        queryResults = await connection.QueryAsync<childType>( query, parent.ID, childIdentifyingPropertyValue);
        //    }
        //    else
        //    {
        //        queryResults = connection.QueryAsync<childType>(query, parent.ID);
        //    }


        //    // Create a typed generic list we can assign back to parent element
        //    IList genericList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

        //    foreach (object item in queryResults)
        //    {
        //        genericList.Add(item);
        //    }

        //    return genericList;

        //}

        private async Task<IList<T>> GetChildren(IBlueSphereEntity parent, string childIdentifyingPropertyName, object childIdentifyingPropertyValue, SQLiteAsyncConnection connection)
        {
            // var t = Activator.CreateInstance<childType>();
            //TableMapping tableMapping =await connection.GetMappingAsync();
            var childType = typeof(T);

            string query = string.Format("select * from {0} where {1} = ?", childType.GetTableName(),
                                                                            childType.GetForeignKeyName(parent.GetType()));

            if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
            {
                query = query + string.Format(" AND {0} = ?", childIdentifyingPropertyName);
            }
            //var listType = typeof(List<>);
            //var constructedListType = listType.MakeGenericType(childType);
            //var queryResults = Activator.CreateInstance(constructedListType);
            //var t = Activator.CreateInstance(childType);

            List<T> queryResults = new List<T>();

            if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
            {
                queryResults = await connection.QueryAsync<T>(query, parent.ID, childIdentifyingPropertyValue);
            }
            else
            {
                queryResults = await connection.QueryAsync<T>(query, parent.ID);
            }


            //// Create a typed generic list we can assign back to parent element
            //IList genericList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

            //foreach (object item in queryResults)
            //{
            //    genericList.Add(item);
            //}

            return queryResults;

        }


        // Deletes all items from the table associated with the specified type
        private void DeleteAllFromTable(Type type, SQLiteConnection connection)
        {
            string command = string.Format("delete from {0}", type.GetTableName());
            connection.CreateCommand(command).ExecuteNonQuery();
        }

        private async Task DeleteAllFromTableAsync(Type type, SQLiteAsyncConnection connection)
        {
            string command = string.Format("delete from {0}", type.GetTableName());
            await connection.ExecuteAsync(command);
        }




        // Ensures the property on the child labelled with the foreign key attribute lines up with
        // the id of the parent
        private static void SetForeignKeyOnChild(IBlueSphereEntity parent, IBlueSphereEntity child)
        {
            PropertyInfo foreignKeyPropInfo = child.GetType().GetForeignKeyProperty(parent.GetType());
            foreignKeyPropInfo.SetValue(child, parent.ID);
        }

        /// <summary>
        /// Performs some action (e.g. insert or delete) recursively on the hierarchical object graph using using the
        ///  ChildRelationship and ForeignKey attributes to guide the process. Deals with the cases where child properties
        ///  are lists or single items.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationshipProperty"></param>
        /// <param name="action"></param>
        private void DoRecursiveTreeAction(IBlueSphereEntity entity, PropertyInfo relationshipProperty, Action<IBlueSphereEntity, IBlueSphereEntity> action)
        {

            if (relationshipProperty.GetCardinalityOfChildRelation() == RelationshipCardinality.OneToOne)
            {
                //Child Property is a single Bluesphere entity
                IBlueSphereEntity singleChild = relationshipProperty.GetValue(entity, null) as IBlueSphereEntity;

                if (singleChild == null)
                {
                    throw new ArgumentException(string.Format("{0} type property {1} is not a  BlueSphere entity", entity.GetType().ToString(), relationshipProperty.Name));
                }

                action.Invoke(entity, singleChild);
            }
            else if (relationshipProperty.GetCardinalityOfChildRelation() == RelationshipCardinality.OneToZeroOrOne)
            {
                //Child Property is a single Bluesphere entity but may be null
                var singleChild = relationshipProperty.GetValue(entity, null);

                if (singleChild != null)
                {
                    var singleChildBlueSphereEntity = singleChild as IBlueSphereEntity;

                    if (singleChildBlueSphereEntity == null)
                    {
                        throw new ArgumentException(string.Format("{0} type property {1} is not a  BlueSphere entity", entity.GetType().ToString(), relationshipProperty.Name));
                    }

                    action.Invoke(entity, singleChildBlueSphereEntity);
                }
            }
            else
            {
                // Child property is a collection of Bluesphere entities
                IEnumerable<IBlueSphereEntity> children = relationshipProperty.GetValue(entity, null) as IEnumerable<IBlueSphereEntity>;
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        action.Invoke(entity, child);
                    }
                }
            }
        }

        #endregion

    }

}
