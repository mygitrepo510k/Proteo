using System;
using SQLite.Net;

namespace MWF.Mobile.Core.Services
{
    public interface IDataService
    {
        SQLiteConnection GetDBConnection();
        SQLite.Net.Async.SQLiteAsyncConnection GetAsyncDBConnection();
        void RunInTransaction(Action action);
        string DatabasePath { get; }
    }
}
