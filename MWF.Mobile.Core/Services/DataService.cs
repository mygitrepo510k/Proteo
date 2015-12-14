using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Attributes;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using SQLite.Net.Platform.Generic;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{

    public class DataService : IDataService
    {

        #region Private Members

        private readonly IDeviceInfo _deviceInfo = null;
        private const string DBNAME = "db.sql";
        private SQLite.Net.SQLiteConnectionString _connectionString = null;
        private string _path = "";
        private SQLitePlatformGeneric _platform = null;
        private SQLiteConnectionPool _connectionPool = null;

        #endregion

        #region Construction

        public DataService(IDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
            _path = Path.Combine(_deviceInfo.DatabasePath, DBNAME);
            _platform = new SQLite.Net.Platform.Generic.SQLitePlatformGeneric();
            _connectionString = new SQLiteConnectionString(_path, true);
            _connectionPool = new SQLiteConnectionPool(_platform);

            CreateTablesIfRequired();
        }

        #endregion

        #region Public Methods

        public async Task RunInTransactionAsync(Action<SQLiteConnection> action)
        {
            var conn = this.GetAsyncDBConnection();
            await conn.RunInTransactionAsync(action);
        }

        #endregion

        #region Properties

        public SQLite.Net.SQLiteConnection GetDBConnection()
        {
            var _conn = new SQLiteConnection(_platform, _path, SQLite.Net.Interop.SQLiteOpenFlags.ReadWrite | SQLite.Net.Interop.SQLiteOpenFlags.Create | SQLite.Net.Interop.SQLiteOpenFlags.FullMutex);            
            return _conn;
        }

        public SQLiteAsyncConnection GetAsyncDBConnection()
        {
            return new SQLiteAsyncConnection(() => _connectionPool.GetConnection(_connectionString));
        }

        public string DatabasePath
        {
            get
            {
                return GetDBConnection().DatabasePath;
            }
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Creates all the tables required for local storage.
        /// If tables already exist connection.CreateTable handles
        /// this gracefully
        /// </summary>
        private void CreateTablesIfRequired()
        {
            var connection = this.GetAsyncDBConnection();
            connection.CreateTableAsync<Additional>();
            connection.CreateTableAsync<Models.Instruction.Address>();
            connection.CreateTableAsync<ApplicationProfile>();
            connection.CreateTableAsync<ConfirmQuantity>();
            connection.CreateTableAsync<CurrentDriver>();
            connection.CreateTableAsync<Customer>();
            connection.CreateTableAsync<DeliveryDescription>();
            connection.CreateTableAsync<Device>();
            connection.CreateTableAsync<Driver>();
            connection.CreateTableAsync<GatewayQueueItem>();
            connection.CreateTableAsync<Instruction>();
            connection.CreateTableAsync<Image>();
            connection.CreateTableAsync<ItemAdditional>();
            connection.CreateTableAsync<Item>();
            connection.CreateTableAsync<LatestSafetyCheck>();
            connection.CreateTableAsync<LogMessage>();
            connection.CreateTableAsync<MWFMobileConfig>();
            connection.CreateTableAsync<MobileData>();
            connection.CreateTableAsync<Order>();
            connection.CreateTableAsync<SafetyCheckData>();
            connection.CreateTableAsync<SafetyCheckFault>();
            connection.CreateTableAsync<SafetyCheckFaultType>();
            connection.CreateTableAsync<SafetyProfile>();
            connection.CreateTableAsync<Signature>();
            connection.CreateTableAsync<Vehicle>();
            connection.CreateTableAsync<Models.Trailer>();
            connection.CreateTableAsync<Models.Instruction.Trailer>();
            connection.CreateTableAsync<VehicleView>();
            connection.CreateTableAsync<VerbProfile>();
            connection.CreateTableAsync<VerbProfileItem>();
        }

        #endregion


    }

}
