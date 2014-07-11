using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Community.Plugins.Sqlite.Wpf;
using Xunit;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using MWF.Mobile.Core.Repositories;

namespace MWF.Mobile.Tests.RepositoryTests
{
    // Tests a full write and read to a sqlite database via the repository and dataservice layers
    // Note this uses a concrete ConnectionFactory targeting x86 windows
    public class RepositoryIntegrationTest
        : MvxIoCSupportingTest, IDisposable
    {

        private IDataService _dataService;

        protected override void AdditionalSetup()
        {


            ISQLiteConnectionFactory connectionFactory = new MvxWpfSqLiteConnectionFactory();
            _dataService = new DataService(connectionFactory);
        }

        [Fact]
        public void Repository_Insert_Read()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            List<Device> devicesIn = fixture.CreateMany<Device>().OrderBy(d => d.ID).ToList();

            DeviceRepository deviceRepository = new DeviceRepository(_dataService);

            // Insert records
            foreach (var device in devicesIn)
            {
                deviceRepository.Insert(device);
            }

            //read them all back
            List<Device> devicesOut = deviceRepository.GetAll().OrderBy(d => d.ID).ToList();


            // Check we got the same number of records out that we put in
            Assert.Equal(devicesIn.Count, devicesIn.Count);

            //Check that all items/properties have the same values
            for (int i = 0; i < devicesIn.Count; i++)
            {
                Assert.Equal(devicesIn[i].ID, devicesOut[i].ID);
                Assert.Equal(devicesIn[i].Title, devicesOut[i].Title);
                Assert.Equal(devicesIn[i].Type, devicesOut[i].Type);
                Assert.Equal(devicesIn[i].CustomerID, devicesOut[i].CustomerID);
                Assert.Equal(devicesIn[i].CustomerTitle, devicesOut[i].CustomerTitle);
                Assert.Equal(devicesIn[i].DeviceIdentifier, devicesOut[i].DeviceIdentifier);
            }

        }

        [Fact]
        public void Repository_Insert_GetByID()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var deviceIn = fixture.Create<Device>();
            Guid ID = deviceIn.ID;

            DeviceRepository deviceRepository = new DeviceRepository(_dataService);

            // Insert record
            deviceRepository.Insert(deviceIn);

            // Get the device back by id
            var deviceOut = deviceRepository.GetByID(ID);

            //Check that all items/properties have the same values
            Assert.Equal(deviceIn.Title, deviceOut.Title);
            Assert.Equal(deviceIn.Type, deviceOut.Type);
            Assert.Equal(deviceIn.CustomerID, deviceOut.CustomerID);
            Assert.Equal(deviceIn.CustomerTitle, deviceOut.CustomerTitle);
            Assert.Equal(deviceIn.DeviceIdentifier, deviceOut.DeviceIdentifier);


        }


        [Fact]
        public void RepositoryWithChildren_Insert_GetByID()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var verbProfile1In = fixture.Create<VerbProfile>();
            var verbProfile2In = fixture.Create<VerbProfile>();

            foreach (var verbProfileItem in verbProfile1In.Children)
            {
                verbProfileItem.VerbProfileID = verbProfile1In.ID;
            }

            VerbProfileRepository verbProfileRepository = new VerbProfileRepository(_dataService);


            // Insert records
            verbProfileRepository.Insert(verbProfile1In);
            verbProfileRepository.Insert(verbProfile2In);

            // Get the first profile back by id
            var verbProfile1Out = verbProfileRepository.GetByID(verbProfile1In.ID);

            // Check that verb profile we retreived has correct number of children
            Assert.Equal(verbProfile1In.Children.Count, verbProfile1Out.Children.Count);

            //Check that all the children we have retreived have the correct property values
            for (int i = 0; i < verbProfile1In.Children.Count; i++)
            {
                Assert.Equal(verbProfile1In.Children[i].ID, verbProfile1Out.Children[i].ID);
                Assert.Equal(verbProfile1In.Children[i].Title, verbProfile1Out.Children[i].Title);
                Assert.Equal(verbProfile1In.Children[i].Category, verbProfile1Out.Children[i].Category);
                Assert.Equal(verbProfile1In.Children[i].Code, verbProfile1Out.Children[i].Code);
                Assert.Equal(verbProfile1In.Children[i].IsHighlighted, verbProfile1Out.Children[i].IsHighlighted);
                Assert.Equal(verbProfile1In.Children[i].Order, verbProfile1Out.Children[i].Order);
                Assert.Equal(verbProfile1In.Children[i].ShowComment, verbProfile1Out.Children[i].ShowComment);
                Assert.Equal(verbProfile1In.Children[i].ShowImage, verbProfile1Out.Children[i].ShowImage);
                Assert.Equal(verbProfile1In.Children[i].ShowSignature, verbProfile1Out.Children[i].ShowSignature);
            }

        }

        #region IDisposable (Teardown)

        public void Dispose()
        {
            if (_dataService != null) _dataService.Dispose();

            if (File.Exists("db.sql"))
            {
                File.Delete("db.sql");
            }
        }

        #endregion

    }
}
