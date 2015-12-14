using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using SQLite.Net.Attributes;
using Xunit;

namespace MWF.Mobile.Tests.ServiceTests
{

    //public class DataServiceTests
    //    : MvxIoCSupportingTest
    //{


    //    [Fact]
    //    public void DataService_ConnectionIsNotNull()
    //    {
    //        base.ClearAll();

    //        var asyncConnectionMock = new Mock<Core.Database.IAsyncConnection>();
    //        asyncConnectionMock.Setup(c => c.CreateTableAsync<T>());

    //        var deviceInfo = new Mock<IDeviceInfo>().Object;
    //        var dataService = new DataService(deviceInfo);
    //        var connection = dataService.GetDBConnection();

    //        Assert.NotNull(connection);
    //    }

    //    [Fact]
    //    public void DataService_CreateTables()
    //    {
    //        base.ClearAll();

    //        var asyncConnectionMock = new Mock<Core.Database.IAsyncConnection>();
    //        asyncConnectionMock.Setup(c => c.CreateTableAsync<T>());

    //        var deviceInfo = new Mock<IDeviceInfo>().Object;
    //        var dataService = new DataService(deviceInfo);
    //        var connection = dataService.GetDBConnection();

    //        // Check that the various tables has been created
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<ApplicationProfile>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<CurrentDriver>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Customer>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Driver>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Device>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<GatewayQueueItem>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Image>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<SafetyCheckData>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<SafetyCheckFault>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<SafetyProfile>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Signature>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Trailer>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<Vehicle>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<VehicleView>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<VerbProfile>(), Times.Once);
    //        asyncConnectionMock.Verify(c => c.CreateTableAsync<VerbProfileItem>(), Times.Once);
            

    //    }

       


    //}

}
