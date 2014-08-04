using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Tests.Helpers
{
    public class MockITableQuery<T> : ITableQuery<T> where T : new()
    {

        public List<T> Items { get; set; }

        #region ITableQuery

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> Where(System.Linq.Expressions.Expression<Func<T, bool>> predExpr)
        {
            var whereItems = Items.Where(predExpr.Compile());
            return new MockITableQuery<T>() { Items = whereItems.ToList() };
        }


        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ISQLiteConnection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public int Count(System.Linq.Expressions.Expression<Func<T, bool>> predExpr)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            return Items.Count;
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> Deferred()
        {
            throw new NotImplementedException();
        }

        public T ElementAt(int index)
        {
            throw new NotImplementedException();
        }

        public T First()
        {
            return Items.First();
        }

        public T FirstOrDefault()
        {
            return Items.FirstOrDefault();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<TResult> Join<TInner, TKey, TResult>(Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<TInner> inner, System.Linq.Expressions.Expression<Func<T, TKey>> outerKeySelector, System.Linq.Expressions.Expression<Func<TInner, TKey>> innerKeySelector, System.Linq.Expressions.Expression<Func<T, TInner, TResult>> resultSelector)
            where TInner : new()
            where TResult : new()
        {
            throw new NotImplementedException();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> OrderBy<U>(System.Linq.Expressions.Expression<Func<T, U>> orderExpr)
        {
            throw new NotImplementedException();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> OrderByDescending<U>(System.Linq.Expressions.Expression<Func<T, U>> orderExpr)
        {
            throw new NotImplementedException();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<TResult> Select<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> selector) where TResult : new()
        {
            throw new NotImplementedException();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> Skip(int n)
        {
            throw new NotImplementedException();
        }

        public Cirrious.MvvmCross.Community.Plugins.Sqlite.ITableQuery<T> Take(int n)
        {
            throw new NotImplementedException();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

    }
}
