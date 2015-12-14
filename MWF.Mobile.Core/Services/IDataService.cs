using System;
using SQLite.Net;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IDataService
    {
        SQLiteConnection GetDBConnection();
        SQLite.Net.Async.SQLiteAsyncConnection GetAsyncDBConnection();
        Task RunInTransactionAsync(Action<SQLite.Net.SQLiteConnection> action);
        string DatabasePath { get; }
    }
}
