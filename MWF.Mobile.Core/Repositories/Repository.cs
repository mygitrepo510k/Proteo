using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Helpers;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Attributes;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.Repositories
{
        public abstract class Repository<T> : IRepository<T> where T : IBlueSphereEntity, new()
        {

            protected IDataService _dataService;


            #region Construction

            public Repository(IDataService dataService)
            {
                _dataService = dataService;
            }

            #endregion

            #region IRepository<T> Members

            public virtual void Insert(T entity, ISQLiteConnection transactionConnection)
            {
                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    connection.RunInTransaction(() =>
                    {
                        InsertRecursive(entity, connection);
                    });
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }


            }

            public virtual void Insert(T entity)
            {
                Insert(entity, null);
            }

            public virtual void Insert(IEnumerable<T> entities, ISQLiteConnection transactionConnection)
            {

                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    foreach (var entity in entities)
                    {
                        InsertRecursive(entity, connection);
                    }
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }


            }

            public virtual void Insert(IEnumerable<T> entities)
            {
                Insert(entities, null);
            }


            public virtual void Update(T entity, ISQLiteConnection transactionConnection)
            {

                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    connection.RunInTransaction(() =>
                    {
                        var existingEntity = GetByID(entity.ID);
                        if (existingEntity != null)
                        {
                            Delete(existingEntity);
                        }

                        InsertRecursive(entity, connection);
                    });
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }
            
            }

            public virtual void Update(T entity)
            {
                Update(entity, null);
            }


            public virtual void DeleteAll(ISQLiteConnection transactionConnection)
            {
                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    connection.RunInTransaction(() =>
                    {
                        DeleteAllRecursive(typeof(T), connection);
                    });
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }

            }

            public virtual void DeleteAll()
            {
                DeleteAll(null);
            }

            public virtual void Delete(T entity, ISQLiteConnection transactionConnection)
            {
                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    connection.RunInTransaction(() =>
                    {
                        DeleteRecursive(entity, connection);
                    });
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }

            }

            public virtual void Delete(T entity)
            {
                Delete(entity, null);
            }


            public virtual IEnumerable<T> GetAll(ISQLiteConnection transactionConnection)
            {

                List<T> entities;

                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    entities = connection.Table<T>().ToList();
                    if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entities, connection);
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }
              
                return entities;
            }

            public virtual IEnumerable<T> GetAll()
            {
                return GetAll(null);
            }


            public virtual T GetByID(Guid ID, ISQLiteConnection transactionConnection)
            {
                T entity;

                ISQLiteConnection connection = transactionConnection ?? _dataService.GetDBConnection();

                try
                {
                    entity = connection.Table<T>().SingleOrDefault(e => e.ID == ID);

                    if (entity != null)
                        if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entity, connection);
                }
                finally
                {
                    // if we created our own connection then close it
                    if (transactionConnection == null)
                        connection.Close();
                }

                return entity;
            }

            public virtual T GetByID(Guid ID)
            {
                return GetByID(ID, null);
            }

            #endregion

            #region Private Properties

            #endregion

            #region Private Methods

            /// <summary>
            ///  Inserts a potentially nested object graph into the appropriate tables using the
            ///  ChildRelationship and ForeignKey attributes to guide the process
            /// </summary>
            /// <param name="entity"></param>
            private void InsertRecursive(IBlueSphereEntity entity, ISQLiteConnection connection)
            {

                connection.Insert(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    DoRecursiveTreeAction(entity, relationshipProperty, (parent, child) =>
                    {
                        SetForeignKeyOnChild(parent, child);
                        InsertRecursive(child, connection);
                    });
                }
            }


            /// <summary>
            ///  Deletes a potentially nested object graph from the appropriate tables using the
            ///  ChildRelationship and ForeignKey attributes to guide the process
            /// </summary>
            /// <param name="entity"></param>
            private void DeleteRecursive(IBlueSphereEntity entity, ISQLiteConnection connection)
            {
                connection.Delete(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    DoRecursiveTreeAction(entity, relationshipProperty, (parent, child) =>
                    {
                        DeleteRecursive(child, connection);
                    });

                }
            }

            /// <summary>
            /// Deletes all items of the specified type from their respective tables
            /// plus any child types as specified by the ChildRelationship attribute
            /// </summary>
            /// <param name="type"></param>
            private void DeleteAllRecursive(Type type, ISQLiteConnection connection)
            {
                DeleteAllFromTable(type, connection);

                foreach (var childType in type.GetChildRelationTypes())
                {

                    DeleteAllRecursive(childType, connection);

                }
            }
          

            /// <summary>
            /// For the parent entity pulled from a table, populates any children
            /// as labelled with the ChildRelationship. 
            /// </summary>
            /// <param name="parent"></param>
            protected void PopulateChildrenRecursive(IBlueSphereEntity parent, ISQLiteConnection connection)
            {

                foreach (var relationshipProperty in parent.GetType().GetChildRelationProperties())
                {
                    Type childType = relationshipProperty.GetTypeOfChildRelation();
                    string childIdentifyingPropertyName = relationshipProperty.GetIdentifyingPropertyNameOfChildRelation();
                    object childIdentifyingPropertyValue = relationshipProperty.GetIdentifyingPropertyValueOfChildRelation();
                    IList children = GetChildren(parent, childType, childIdentifyingPropertyName, childIdentifyingPropertyValue, connection);

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

                    PopulateChildrenRecursive(children, connection);

                }
            }

            /// <summary>
            /// Overload of PopulateChildrenRecursive that deals with
            /// a collection of parents
            /// </summary>
            /// <param name="parents"></param>
            protected void PopulateChildrenRecursive(IEnumerable parents, ISQLiteConnection connection)
            {
                foreach (var parent in parents)
                {
                    PopulateChildrenRecursive(parent as IBlueSphereEntity, connection);
                }
            }

            // Gets the children of the specfied type for specified parent using foreign key mappings
            private IList GetChildren(IBlueSphereEntity parent, Type childType, string childIdentifyingPropertyName, object childIdentifyingPropertyValue, ISQLiteConnection connection)
            {
                ITableMapping tableMapping = connection.GetMapping(childType);

                string query = string.Format("select * from {0} where {1} = ?", childType.GetTableName(),
                                                                                childType.GetForeignKeyName(parent.GetType()));

                if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
                {
                    query = query + string.Format(" AND {0} = ?", childIdentifyingPropertyName);
                }

                List<object> queryResults;

                if (!string.IsNullOrEmpty(childIdentifyingPropertyName))
                {
                    queryResults = connection.Query(tableMapping, query, parent.ID, childIdentifyingPropertyValue);
                }
                else
                {
                    queryResults = connection.Query(tableMapping, query, parent.ID);
                }


                // Create a typed generic list we can assign back to parent element
                IList genericList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(childType));

                foreach (object item in queryResults)
                {
                    genericList.Add(item);
                }

                return genericList;

            }

            // Deletes all items from the table associated with the specified type
            private void DeleteAllFromTable(Type type, ISQLiteConnection connection)
            {
                string command = string.Format("delete from {0}", type.GetTableName());
                connection.CreateCommand(command).ExecuteNonQuery();
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
