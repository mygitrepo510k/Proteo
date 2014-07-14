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
using MWF.Mobile.Tests.RepositoryTests.TestModels;

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

            _dataService.Connection.CreateTable<GrandParentEntity>();
            _dataService.Connection.CreateTable<ParentEntity>();
            _dataService.Connection.CreateTable<ChildEntity>();
            _dataService.Connection.CreateTable<ChildEntity2>();
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
        // Tests a repository can deal with an entity type which has a child relationship
        // with an other entity type
        public void Repository_SingleChildRelation_Insert_GetByID()
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

        [Fact]
        // Tests a repository can deal with an entity type which has a child relationships
        // with more than one entity type
        public void Repository_MultipleChildRelation_Insert_GetByID()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var parentEntityIn = fixture.Create<ParentEntity>();
            var parentEntity2In = fixture.Create<ParentEntity>();

            // set up the foreign key relationships
            foreach (var childEntity in parentEntityIn.Children)
            {
                childEntity.ParentID = parentEntityIn.ID;
            }

            foreach (var childEntity in parentEntityIn.Children2)
            {
                childEntity.ParentID = parentEntityIn.ID;
            }

            ParentEntityRepository repository = new ParentEntityRepository(_dataService);


            // Insert records
            repository.Insert(parentEntityIn);
            repository.Insert(parentEntity2In);

            // Get the first entity back by id
            var parentEntityOut = repository.GetByID(parentEntityIn.ID);

            // Check that the entity we retreived has correct number of children (of both types)
            Assert.Equal(parentEntityIn.Children.Count, parentEntityOut.Children.Count);
            Assert.Equal(parentEntityIn.Children2.Count, parentEntityOut.Children2.Count);

            // Check that all the children we have retreived have the correct property values
            for (int i = 0; i < parentEntityIn.Children.Count; i++)
            {
                Assert.Equal(parentEntityIn.Children[i].ID, parentEntityOut.Children[i].ID);
                Assert.Equal(parentEntityIn.Children[i].Title, parentEntityOut.Children[i].Title);
            }

            // Check that all the children we have retreived have the correct property values
            for (int i = 0; i < parentEntityIn.Children2.Count; i++)
            {
                Assert.Equal(parentEntityIn.Children2[i].ID, parentEntityOut.Children2[i].ID);
                Assert.Equal(parentEntityIn.Children2[i].Title, parentEntityOut.Children2[i].Title);

            }

        }

        [Fact]
        // Tests a repository can deal with an entity type which has nested child relationships
        // e.g. Grandparent -> Parent -> Child
        public void Repository_NestedChildRelation_Insert_GetByID()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var grandParentEntityIn = fixture.Create<GrandParentEntity>();
            var grandParentEntity2In = fixture.Create<GrandParentEntity>();

            // set up the foreign key relationships
            foreach (var parentEntity in grandParentEntityIn.Children)
            {
                parentEntity.ParentID = grandParentEntityIn.ID;
                foreach (var childEntity in parentEntity.Children)
                {
                    childEntity.ParentID = parentEntity.ID;
                    parentEntity.Children2.Clear();                  
                }              
            }

            GrandParentEntityRepository repository = new GrandParentEntityRepository(_dataService);

            // Insert records
            repository.Insert(grandParentEntityIn);
            repository.Insert(grandParentEntity2In);

            // Get the first entity back by id
            var grandParentEntityOut = repository.GetByID(grandParentEntityIn.ID);

            // Check that the entity we retreived has correct number of children 
            Assert.Equal(grandParentEntityIn.Children.Count, grandParentEntityOut.Children.Count);

            // Check down the hierarchy that property values line up
            for (int i = 0; i < grandParentEntityIn.Children.Count; i++)
            {
                Assert.Equal(grandParentEntityIn.Children[i].ID, grandParentEntityOut.Children[i].ID);
                Assert.Equal(grandParentEntityIn.Children[i].Title, grandParentEntityOut.Children[i].Title);

                // Check that the chidlren have correct number of children 
                Assert.Equal(grandParentEntityIn.Children[i].Children.Count, grandParentEntityOut.Children[i].Children.Count);

                for (int j = 0; j < grandParentEntityIn.Children[i].Children.Count; j++)
                {
                    Assert.Equal(grandParentEntityIn.Children[i].Children[j].ID, grandParentEntityOut.Children[i].Children[j].ID);
                    Assert.Equal(grandParentEntityIn.Children[i].Children[j].Title, grandParentEntityOut.Children[i].Children[j].Title);
                }

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

    #region Test Repository Classes

    internal class ParentEntityRepository : Repository<ParentEntity>
    {
        public ParentEntityRepository(IDataService dataService)
            : base(dataService)
        { }
    }

    internal class GrandParentEntityRepository : Repository<GrandParentEntity>
    {
        public GrandParentEntityRepository(IDataService dataService)
            : base(dataService)
        { }
    }

    #endregion

}
