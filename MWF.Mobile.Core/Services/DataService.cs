using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using System;


namespace MWF.Mobile.Core.Services
{

    public class DataService : IDataService
    {

        #region Private Members

        private ISQLiteConnectionFactory _connectionFactory;
        private const string DBNAME = "db.sql";
        //private bool disposed = false; 

        #endregion

        #region Construction

        public DataService(ISQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            using (var connection = GetDBConnection())
            {
                CreateTablesIfRequired(connection);
            }

        }

        #endregion

        #region Public Methods

        public void RunInTransaction(Action<ISQLiteConnection> action)
        {
            using (var connection = this.GetDBConnection())
            {
                action(connection);
            }          
        }

        #endregion

        #region Properties

        public ISQLiteConnection GetDBConnection()
        {
           return _connectionFactory.Create(DBNAME);
        }
        
        #endregion

        #region Private Methods


        /// <summary>
        /// Creates all the tables required for local storage.
        /// If tables already exist connection.CreateTable handles
        /// this gracefully
        /// </summary>
        private void CreateTablesIfRequired(ISQLiteConnection connection)
        {
            connection.CreateTable<Additional>();
            connection.CreateTable<Models.Instruction.Address>();
            connection.CreateTable<ApplicationProfile>();
            connection.CreateTable<ConfirmQuantity>();
            connection.CreateTable<CurrentDriver>();
            connection.CreateTable<Customer>();
            connection.CreateTable<DeliveryDescription>();
            connection.CreateTable<Device>();
            connection.CreateTable<Driver>();
            connection.CreateTable<GatewayQueueItem>();
            connection.CreateTable<Instruction>();
            connection.CreateTable<Image>();
            connection.CreateTable<ItemAdditional>();
            connection.CreateTable<Item>();
            connection.CreateTable<LatestSafetyCheck>();
            connection.CreateTable<LogMessage>();
            connection.CreateTable<MWFMobileConfig>();
            connection.CreateTable<MobileData>();
            connection.CreateTable<Order>();
            connection.CreateTable<SafetyCheckData>();
            connection.CreateTable<SafetyCheckFault>();
            connection.CreateTable<SafetyCheckFaultType>();
            connection.CreateTable<SafetyProfile>();
            connection.CreateTable<Signature>();
            connection.CreateTable<Vehicle>();
            connection.CreateTable<Models.Trailer>();
            connection.CreateTable<Models.Instruction.Trailer>();
            connection.CreateTable<VehicleView>();
            connection.CreateTable<VerbProfile>();
            connection.CreateTable<VerbProfileItem>();
        }

        #endregion

        //#region IDisposable

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            _connection.Close();
        //        }

        //        disposed = true;
        //    }
        //}

        //#endregion


    }

}
