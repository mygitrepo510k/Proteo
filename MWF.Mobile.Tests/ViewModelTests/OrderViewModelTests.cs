using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class OrderViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mobileDataRepo;
        private Mock<INavigationService> _navigationService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void OrderVM_OrderID()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().ItemIdFormatted, orderVM.OrderID);

        }

        [Fact]
        public void OrderVM_Title()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Title, orderVM.OrderLoadNo);

        }

        [Fact]
        public void OrderVM_DeliveryOrderNumber()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().DeliveryOrderNumber, orderVM.OrderDeliveryNo);

        }

        [Fact]
        public void OrderVM_Quantity()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Quantity, orderVM.OrderQuantity);

        }

        [Fact]
        public void OrderVM_Weight()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Weight, orderVM.OrderWeight);

        }

        [Fact]
        public void OrderVM_BusinessType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().BusinessType, orderVM.OrderBusinessType);

        }

        [Fact]
        public void OrderVM_GoodsType()
        {
            base.ClearAll();

            var orderVM = _fixture.Create<OrderViewModel>();

            orderVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().GoodsType, orderVM.OrderGoodsType);

        }

        #endregion Tests
    }
}
