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
        : MvxIoCSupportingTest
    {

        private IDataService _dataService;

        protected override void AdditionalSetup()
        {

            ISQLiteConnectionFactory connectionFactory = new MvxWpfSqLiteConnectionFactory();


            if (File.Exists("db.sql"))
            {
                _dataService = new DataService(connectionFactory);
                _dataService.GetDBConnection().DeleteAll<Device>();
                _dataService.GetDBConnection().DeleteAll<GrandParentEntity>();
                _dataService.GetDBConnection().DeleteAll<ParentEntity>();
                _dataService.GetDBConnection().DeleteAll<ChildEntity>();
                _dataService.GetDBConnection().DeleteAll<ChildEntity2>();
                _dataService.GetDBConnection().DeleteAll<SingleChildEntity>();
                _dataService.GetDBConnection().CreateTable<MultiChildEntity>();
            }
            else
            {
                _dataService = new DataService(connectionFactory);
                _dataService.GetDBConnection().CreateTable<Device>();
                _dataService.GetDBConnection().CreateTable<GrandParentEntity>();
                _dataService.GetDBConnection().CreateTable<ParentEntity>();
                _dataService.GetDBConnection().CreateTable<ChildEntity>();
                _dataService.GetDBConnection().CreateTable<ChildEntity2>();
                _dataService.GetDBConnection().CreateTable<SingleChildEntity>();
                _dataService.GetDBConnection().CreateTable<MultiChildEntity>();
            }

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

            parentEntityIn.FirstChild.IsFirstChild = true;
            parentEntityIn.SecondChild.IsFirstChild = false;

            var parentEntity2In = fixture.Create<ParentEntity>();

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

            // Check single child relationship
            Assert.Equal(parentEntityIn.Child.ID, parentEntityOut.Child.ID);
            Assert.Equal(parentEntityIn.Child.Title, parentEntityOut.Child.Title);

            // Check multi child relationship
            Assert.Equal(parentEntityIn.FirstChild.ID, parentEntityOut.FirstChild.ID);
            Assert.Equal(parentEntityIn.FirstChild.Title, parentEntityOut.FirstChild.Title);
            Assert.Equal(parentEntityIn.FirstChild.IsFirstChild, parentEntityOut.FirstChild.IsFirstChild);

            Assert.Equal(parentEntityIn.SecondChild.ID, parentEntityOut.SecondChild.ID);
            Assert.Equal(parentEntityIn.SecondChild.Title, parentEntityOut.SecondChild.Title);
            Assert.Equal(parentEntityIn.SecondChild.IsFirstChild, parentEntityOut.SecondChild.IsFirstChild);

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

            GrandParentEntityRepository repository = new GrandParentEntityRepository(_dataService);

            // Insert records
            repository.Insert(grandParentEntityIn);
            repository.Insert(grandParentEntity2In);

            // Get the first entity back by id
            var grandParentEntityOut = repository.GetByID(grandParentEntityIn.ID);

            CheckEntityTreesAreSame(grandParentEntityIn, grandParentEntityOut);


        }

        [Fact]
        public void Repository_NestedChildRelation_InsertMany_GetAll()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            GrandParentEntityRepository repository = new GrandParentEntityRepository(_dataService);

            // Insert records
            List<GrandParentEntity> grandParentEntitiesIn = fixture.CreateMany<GrandParentEntity>().ToList();
            repository.Insert(grandParentEntitiesIn);

            // Get all the entities out
            var grandParentEntitiesOut = repository.GetAll().ToList();

            // Check that we got the same number of entities back out
            Assert.Equal(grandParentEntitiesIn.ToList().Count, grandParentEntitiesOut.ToList().Count);

            // Check down the hierarchy that property values line up
            for (int i = 0; i < grandParentEntitiesIn.Count; i++)
            {
                CheckEntityTreesAreSame(grandParentEntitiesIn[i], grandParentEntitiesOut[i]);
            }
            
        }

        [Fact]
        // Tests a repository can deal with an entity type which has nested child relationships
        // e.g. Grandparent -> Parent -> Child
        public void Repository_NestedChildRelation_DeleteAll()
        {
            base.ClearAll();

            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var grandParentEntityIn = fixture.Create<GrandParentEntity>();


            GrandParentEntityRepository repository = new GrandParentEntityRepository(_dataService);

            // Insert records
            repository.Insert(grandParentEntityIn);

            // DeleteAll
            repository.DeleteAll();

            // Check the table and all its child tables are now empty
            Assert.Empty(_dataService.GetDBConnection().Table<GrandParentEntity>());
            Assert.Empty(_dataService.GetDBConnection().Table<ParentEntity>());
            Assert.Empty(_dataService.GetDBConnection().Table<ChildEntity>());
            Assert.Empty(_dataService.GetDBConnection().Table<ChildEntity2>());
            Assert.Empty(_dataService.GetDBConnection().Table<SingleChildEntity>());
        }

        #region helper functions

        private void CheckEntityTreesAreSame(GrandParentEntity grandParentEntityIn, GrandParentEntity grandParentEntityOut)
        {
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

                // Check single child relationship
                Assert.Equal(grandParentEntityIn.Children[i].Child.ID, grandParentEntityOut.Children[i].Child.ID);
                Assert.Equal(grandParentEntityIn.Children[i].Child.Title, grandParentEntityOut.Children[i].Child.Title);

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
