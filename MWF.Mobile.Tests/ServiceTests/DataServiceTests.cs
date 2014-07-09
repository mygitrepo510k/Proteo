using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    public class DataServiceTests
        : MvxIoCSupportingTest
    {


        [Fact]
        public void DataService_ConnectionIsNotNull()
        {
            base.ClearAll();

            var connectionFactoryMock = new Mock<ISQLiteConnectionFactory>();
            var connectionMock = new Mock<ISQLiteConnection>();
            connectionFactoryMock.Setup(cf => cf.Create(It.Is<string>(s => s == "db.sql"))).Returns(connectionMock.Object);


            DataService dataService = new DataService(connectionFactoryMock.Object);
            ISQLiteConnection connection = dataService.Connection;

            Assert.NotNull(connection);

        }

        [Fact]
        public void DataService_CreateCustomerTable()
        {
            base.ClearAll();

            var connectionFactoryMock = new Mock<ISQLiteConnectionFactory>();
            var connectionMock = new Mock<ISQLiteConnection>();

            connectionMock.Setup(c => c.CreateTable<Customer>(CreateFlags.None));
            connectionFactoryMock.Setup(cf => cf.Create(It.Is<string>(s => s == "db.sql"))).Returns(connectionMock.Object);

            DataService dataService = new DataService(connectionFactoryMock.Object);
            ISQLiteConnection connection = dataService.Connection;

            // Check that the customer table has been created
            connectionMock.Verify(c => c.CreateTable<Customer>(CreateFlags.None), Times.Once);
    

        }

       


    }

}
