using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.RepositoryTests
{

    // Tests the BaseRepository class (albeit by using a concrete subclass)
    // Note these are fairly basic tests. End to End integration tests using an
    // actual sqlite database are in RepositoryIntegrationTests.cs.

    public class RepositoryTests
        : MvxIoCSupportingTest
    {

        private Mock<ISQLiteConnection> _connectionMock;
        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _connectionMock = new Mock<ISQLiteConnection>();
            _connectionMock.Setup(c => c.RunInTransaction(It.IsAny<Action>())).Callback( (Action a) => a.Invoke() );

            var dataServiceMock = Mock.Of<IDataService>(ds => ds.GetDBConnection() == _connectionMock.Object);
            _fixture.Register<IDataService>(() => dataServiceMock);
            
        }

        [Fact]
        public void Repository_Insert()
        {
            base.ClearAll();

            var deviceRepository = _fixture.Create<DeviceRepository>();
            var device = _fixture.Create<Device>();

            deviceRepository.Insert(device);

            // SQL connection should have been hit with an insert
            _connectionMock.Verify(c => c.Insert(It.Is<Device>( d => Object.Equals(d,device) )),  Times.Once);
    
        }

        [Fact]
        public void Repository_InsertMany()
        {
            base.ClearAll();

            var deviceRepository = _fixture.Create<DeviceRepository>();
            var devices = _fixture.CreateMany<Device>().ToList();

            deviceRepository.Insert(devices);

            // SQL connection should have been hit with an insert for each device
            _connectionMock.Verify(c => c.Insert(It.IsAny<Device>()), Times.Exactly(devices.Count)); 

            

        }


        [Fact]
        public void Repository_Delete()
        {
            base.ClearAll();

            var deviceRepository = _fixture.Create<DeviceRepository>();
            var device = _fixture.Create<Device>();

            deviceRepository.Delete(device);

            // SQL connection should have been hit with a delete
            _connectionMock.Verify(c => c.Delete(It.Is<Device>(d => Object.Equals(d, device))), Times.Once);

        }

        [Fact]
        public void Repository_GetAll()
        {

            base.ClearAll();

            var devices = _fixture.Create<ITableQuery<Device>>();
            _connectionMock.Setup(c => c.Table<Device>()).Returns(devices);

            var deviceRepository = _fixture.Create<DeviceRepository>();
            var device = _fixture.Create<Device>();

            IEnumerable<Device> devicesOut = deviceRepository.GetAll();

            // SQL connection should have been hit with a pull from the table
            _connectionMock.Verify(c => c.Table<Device>(), Times.Once);


        }


    }

}
