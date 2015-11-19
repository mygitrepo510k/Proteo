using System;
using SQLite.Net;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IDataService
    {
        SQLiteConnection GetDBConnection();
        SQLite.Net.Async.SQLiteAsyncConnection GetAsyncDBConnection();
        void RunInTransaction(Action action);
        Task RunInTransactionAsync(Action<SQLite.Net.Async.SQLiteAsyncConnection> action);
        string DatabasePath { get; }
    }
}
