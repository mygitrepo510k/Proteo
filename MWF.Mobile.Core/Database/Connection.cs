using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;

namespace MWF.Mobile.Core.Database
{

    internal class Connection : IConnection, IDisposable
    {

        private readonly SQLiteConnection _connection;

        public Connection(SQLitePlatformGeneric sqlitePlatform, string databasePath, SQLiteOpenFlags openFlags)
        {
            _connection = new SQLiteConnection(sqlitePlatform, databasePath, openFlags);
        }

        internal Connection(SQLiteConnection sqliteConnection)
        {
            _connection = sqliteConnection;
        }

        public string DatabasePath
        {
            get { return _connection.DatabasePath; }
        }

        public IEnumerable<T> Table<T>() where T : class
        {
            return _connection.Table<T>();
        }

        public int Insert(object obj)
        {
            return _connection.Insert(obj);
        }

        public int Delete(object objectToDelete)
        {
            return _connection.Delete(objectToDelete);
        }

        public int Execute(string query, params object[] args)
        {
            return _connection.Execute(query, args);
        }

        public TableMapping GetMapping(Type type)
        {
            return _connection.GetMapping(type);
        }

        public IList<object> Query(TableMapping map, string query, params object[] args)
        {
            return _connection.Query(map, query, args);
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }

    }

}
