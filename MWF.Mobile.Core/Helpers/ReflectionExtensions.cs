using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Helpers
{
    public static class ReflectionExtensions
    {

        public static List<PropertyInfo> GetChildRelationProperties(this Type type)
        {
            return (from property in type.GetRuntimeProperties()
                    where property.GetCustomAttribute<ChildRelationshipAttribute>() != null
                    select property).ToList();
        }

        public static bool HasChildRelationProperties(this Type type)
        {
            return type.GetChildRelationProperties().Any();
        }

        public static List<PropertyInfo> GetForeignKeyProperties(this Type type)
        {
            return (from property in type.GetRuntimeProperties()
                    where property.GetCustomAttribute<ForeignKeyAttribute>() != null
                    select property).ToList();
        } 

        public static Type GetTypeOfChildRelation(this PropertyInfo propertyInfo)
        {
            ChildRelationshipAttribute attr = propertyInfo.GetCustomAttribute<ChildRelationshipAttribute>();

            return (attr == null) ? null : attr.ChildType;
        }

        public static string GetTableName(this Type type)
        {
            var tableName = type.Name;
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null && tableAttribute.Name != null)
                tableName = tableAttribute.Name;

            return tableName;
        }

        public static string GetColumnName(this PropertyInfo property)
        {
            var column = property.Name;
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null && columnAttribute.Name != null)
                column = columnAttribute.Name;

            return column;
        }

        public static string GetForeignKeyName(this Type childType, Type parentType)
        {

            var foreignKeyProperties = (from property in childType.GetRuntimeProperties()
                                        where property.GetCustomAttribute<ForeignKeyAttribute>() != null
                                        && (property.GetCustomAttribute<ForeignKeyAttribute>() as ForeignKeyAttribute).ForeignType == parentType
                                        select property);

            //Contract.Assert(foreignKeyProperties.Any(), string.Format("Type {0} does not contain any foreign key references back to parent type {1}", childType.ToString(), parentType.ToString()));

            return foreignKeyProperties.Single().GetColumnName();

        }

    }
}
