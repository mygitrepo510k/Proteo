using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace MWF.Mobile.Core.Services
{
    public interface IDataService : IDisposable
    {
        ISQLiteConnection Connection { get; }
    }
}
