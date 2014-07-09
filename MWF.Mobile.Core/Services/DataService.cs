using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;



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
            _connection.CreateTable<Customer>();
        }

        #endregion

        #region Properties

        public ISQLiteConnection Connection
        {
            get { return _connection; }
        }
        

        #endregion


    }

}
