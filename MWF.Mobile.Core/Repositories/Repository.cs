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

            public virtual void Insert(IEnumerable<T> entities)
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

            public virtual void DeleteAll()
            {
                //Contract.Requires<ArgumentNullException>(entity != null, "entity cannot be null");

                _connection.RunInTransaction(() =>
                {
                    DeleteAllRecursive(typeof(T));
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


            public virtual IEnumerable<T> GetAll()
            {
                var entities = _connection.Table<T>().ToList();

                if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entities);

                return entities;
            }

            public virtual T GetByID(Guid ID)
            {
                T entity = _connection.Table<T>().SingleOrDefault(e => e.ID == ID);

                if (entity != null)
                    if (typeof(T).HasChildRelationProperties()) PopulateChildrenRecursive(entity);

                return entity;
            }

            #endregion

            #region Private Methods

            /// <summary>
            ///  Inserts a potentially nested object graph into the appropriate tables using the
            ///  ChildRelationship and ForeignKey attributes to guide the process
            /// </summary>
            /// <param name="entity"></param>
            private void InsertRecursive(IBlueSphereEntity entity)
            {
                _connection.Insert(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    DoRecursiveTreeAction(entity, relationshipProperty, (parent, child) =>
                    {
                        SetForeignKeyOnChild(parent, child);
                        InsertRecursive(child);
                    });
                }
            }


            /// <summary>
            ///  Deletes a potentially nested object graph from the appropriate tables using the
            ///  ChildRelationship and ForeignKey attributes to guide the process
            /// </summary>
            /// <param name="entity"></param>
            private void DeleteRecursive(IBlueSphereEntity entity)
            {
                _connection.Delete(entity);

                foreach (var relationshipProperty in entity.GetType().GetChildRelationProperties())
                {
                    DoRecursiveTreeAction(entity, relationshipProperty, (parent, child) =>
                    {
                        DeleteRecursive(child);
                    });

                }
            }

            /// <summary>
            /// Deletes all items of the specified type from their respective tables
            /// plus any child types as specified by the ChildRelationship attribute
            /// </summary>
            /// <param name="type"></param>
            private void DeleteAllRecursive(Type type)
            {
                DeleteAllFromTable(type);

                foreach (var childType in type.GetChildRelationTypes())
                {

                    DeleteAllRecursive(childType);

                }
            }
          

            /// <summary>
            /// For the parent entity pulled from a table, populates any children
            /// as labelled with the ChildRelationship. 
            /// </summary>
            /// <param name="parent"></param>
            private void PopulateChildrenRecursive(IBlueSphereEntity parent)
            {

                foreach (var relationshipProperty in parent.GetType().GetChildRelationProperties())
                {
                    Type childType = relationshipProperty.GetTypeOfChildRelation();
                    IList children = GetChildren(parent, childType);

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

                    PopulateChildrenRecursive(children);

                }
            }

            /// <summary>
            /// Overload of PopulateChildrenRecursive that deals with
            /// a collection of parents
            /// </summary>
            /// <param name="parents"></param>
            private void PopulateChildrenRecursive(IEnumerable parents)
            {
                foreach (var parent in parents)
                {
                    PopulateChildrenRecursive(parent as IBlueSphereEntity);
                }
            }

            // Gets the children of the specfied type for specified parent using foreign key mappings
            private IList GetChildren(IBlueSphereEntity parent, Type childType)
            {
                ITableMapping tableMapping = _connection.GetMapping(childType);

                string query = string.Format("select * from {0} where {1} = ?", childType.GetTableName(),
                                                                                childType.GetForeignKeyName(parent.GetType()));


                List<object> queryResults;

                try
                {
                    queryResults = _connection.Query(tableMapping, query, parent.ID);
                }
                catch (Exception ex)
                {
                    
                    throw;
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
            private void DeleteAllFromTable(Type type)
            {
                string command = string.Format("delete from {0}", type.GetTableName());
                _connection.CreateCommand(command).ExecuteNonQuery();
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
                    if (children == null)
                    {
                        throw new ArgumentException(string.Format("{0} type property {1} does not contain BlueSphere entities", entity.GetType().ToString(), relationshipProperty.Name));
                    }

                    foreach (var child in children)
                    {
                        action.Invoke(entity, child);
                    }
                }
            }

            #endregion

        }

}
