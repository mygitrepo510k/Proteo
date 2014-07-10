using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;


namespace MWF.Mobile.Core.Services
{

    public class DataService : MWF.Mobile.Core.Services.IDataService
    {

        #region Private Members

        private ISQLiteConnection _connection;
        private const string DBNAME = "db.sql";

        #endregion

        #region Construction

        public DataService(ISQLiteConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.Create(DBNAME);
            CreateTablesIfRequired();

        }

        #endregion

        #region Properties

        public ISQLiteConnection Connection
        {
            get { return _connection; }
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
            _connection.CreateTable<ApplicationProfile>();
            _connection.CreateTable<Customer>();
            _connection.CreateTable<Device>();
            _connection.CreateTable<Driver>();
            _connection.CreateTable<SafetyCheckFaultType>();
            _connection.CreateTable<SafetyProfile>();
            _connection.CreateTable<Vehicle>();
            _connection.CreateTable<VehicleView>();
            _connection.CreateTable<VerbProfile>();
            _connection.CreateTable<VerbProfileItem>();
        }

        #endregion


    }

}
