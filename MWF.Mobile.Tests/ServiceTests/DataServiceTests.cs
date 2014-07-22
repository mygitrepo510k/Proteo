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
        public void DataService_CreateTables()
        {
            base.ClearAll();

            var connectionFactoryMock = new Mock<ISQLiteConnectionFactory>();
            var connectionMock = new Mock<ISQLiteConnection>();

            connectionMock.Setup(c => c.CreateTable<Customer>(CreateFlags.None));
            connectionFactoryMock.Setup(cf => cf.Create(It.Is<string>(s => s == "db.sql"))).Returns(connectionMock.Object);

            DataService dataService = new DataService(connectionFactoryMock.Object);
            ISQLiteConnection connection = dataService.Connection;

            // Check that the various tables has been created
            connectionMock.Verify(c => c.CreateTable<ApplicationProfile>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<CurrentDriver>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Customer>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Driver>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Device>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<GatewayQueueItem>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Image>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<SafetyCheckData>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<SafetyCheckFault>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<SafetyProfile>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Signature>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Trailer>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<Vehicle>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<VehicleView>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<VerbProfile>(CreateFlags.None), Times.Once);
            connectionMock.Verify(c => c.CreateTable<VerbProfileItem>(CreateFlags.None), Times.Once);
            

        }

       


    }

}
