﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
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
using MWF.Mobile.Core.Repositories.Interfaces;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class BarcodeItemViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ICustomUserInteraction> _mockCustomUserInteraction;
        private Mock<IUserInteraction> _mockUserInteraction;
        private TestBarcodeScanningModelVM _barcodeScanningViewModel;
        private List<DamageStatus> _damageStatuses;
        private Mock<IMvxMessenger> _mockMessenger;
        private MobileData _mobileData;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.Order.Type = InstructionType.Deliver;

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            _mockCustomUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _fixture.Customize<BarcodeScanningViewModel>(vm => vm.Without(x => x.BarcodeSections));
            _fixture.Customize<BarcodeScanningViewModel>(vm => vm.Without(x => x.BarcodeInput));

            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);

            _barcodeScanningViewModel = _fixture.Create<TestBarcodeScanningModelVM>();
            _barcodeScanningViewModel.MarkAsProcessedBarcodeItem = null;
            _barcodeScanningViewModel.MarkAsProcessedWasScanned = null;
            _barcodeScanningViewModel.Init(new NavData<MobileData>() { Data = _mobileData });

            _mockUserInteraction = new Mock<IUserInteraction>();
            Ioc.RegisterSingleton<IUserInteraction>(_mockUserInteraction.Object);

             _damageStatuses = _fixture.CreateMany<DamageStatus>().ToList();
            _damageStatuses[0].Code = "POD";
            _damageStatuses[1].Code = "PODD";

        }

        #endregion Setup

        #region Test
        
        [Fact]
        public void BarcodeItemVM_Construction()
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            // check the damage statuses have been set correctly
            Assert.Equal(_damageStatuses, barcodeItemVM.DamageStatuses);
            Assert.Equal(_damageStatuses[0], barcodeItemVM.DamageStatus);

        }

        [Theory]
        [InlineData(null, "POD", "POD")]
        [InlineData(true, "POD", "POD")]
        [InlineData(false, "POD", "XPODX")]
        public void BarcodeItemVM_PalletForceDeliveryStatus(bool? isDelivered, string damageStatusCode, string expectedPalletForceStatus)
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            barcodeItemVM.IsDelivered = isDelivered;
            barcodeItemVM.DamageStatus = barcodeItemVM.DamageStatuses.Single(ds => ds.Code == damageStatusCode);

            Assert.Equal(expectedPalletForceStatus, barcodeItemVM.PalletforceDeliveryStatus);

        }

        [Theory]
        [InlineData(true, "POD", "", true)]
        [InlineData(true, "POD", "Some comments", true)]
        [InlineData(false, "POD", "Some comments", true)]
        [InlineData(true, "PODD", "Some comments", true)]
        [InlineData(false, "POD", "", false)]
        [InlineData(true, "PODD", "", false)]
        [InlineData(false, "PODD", "", false)]
        public void BarcodeItemVM_ValidComments(bool? isDelivered, string damageStatusCode, string deliveryComments, bool expectedValidComments)
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            barcodeItemVM.IsDelivered = isDelivered;
            barcodeItemVM.DamageStatus = barcodeItemVM.DamageStatuses.Single(ds => ds.Code == damageStatusCode);
            barcodeItemVM.DeliveryComments = deliveryComments;

            Assert.Equal(expectedValidComments, barcodeItemVM.ValidComments);

        }

        [Fact]
        public void BarcodeItemVM_Clone()
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            var clone = barcodeItemVM.Clone();

            // check the properties we care about have been cloned
            Assert.Equal(_damageStatuses, barcodeItemVM.DamageStatuses);
            Assert.Equal(barcodeItemVM.DamageStatus, barcodeItemVM.DamageStatus);
            Assert.Equal(barcodeItemVM.DeliveryComments, barcodeItemVM.DeliveryComments);
            Assert.Equal(barcodeItemVM.IsDelivered, barcodeItemVM.IsDelivered);

        }

        [Fact]
        // Tests that when a processed barcode item is selected it shows the barcode status modal
        public void BarcodeItemVM_Select_SingleSelect_Processed()
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            barcodeItemVM.IsDelivered = true;

            barcodeItemVM.SelectBarcodeCommand.Execute(null);

            _mockNavigationService.Verify(ns => ns.ShowModalViewModel<BarcodeStatusViewModel, bool>(It.Is<BarcodeItemViewModel>(x => x == barcodeItemVM),
                                                                                                     It.Is<NavData<BarcodeItemViewModel>>(x => x.Data == barcodeItemVM),
                                                                                                     It.IsAny<Action<bool>>()));
         
        }

        [Fact]
        // Tests that when a processed barcode item is selected it shows the barcode status modal
        public void BarcodeItemVM_Select_MultiSelect_Processed()
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            barcodeItemVM.IsDelivered = true;

            // on the parent barcode model, mark snother of the other barcode items as processed (and selected)
            var selectedBarcodeItem = _barcodeScanningViewModel.BarcodeSections[0].Barcodes[0];
            _barcodeScanningViewModel.MarkBarcodeAsProcessed(selectedBarcodeItem);
            selectedBarcodeItem.IsSelected = true;

            barcodeItemVM.SelectBarcodeCommand.Execute(null);

            // Check the selected barcode was passed into the modal as the part of the nav data
            _mockNavigationService.Verify(ns => ns.ShowModalViewModel<BarcodeStatusViewModel, bool>(It.Is<BarcodeItemViewModel>(x => x == barcodeItemVM),
                                                                                                     It.Is<NavData<BarcodeItemViewModel>>(x => x.Data == barcodeItemVM &&
                                                                                                                                               (x.OtherData["SelectedBarcodes"] as List<BarcodeItemViewModel>)[0] == selectedBarcodeItem),
                                                                                                     It.IsAny<Action<bool>>()));

        }

        [Fact]
        // Tests that when an unprocessed barcode item is selected it shows the confirm dialog then calls "MarkAsProcessed" on the parent barcode scanning view model
        public void BarcodeItemVM_Select_Unprocessed()
        {
            base.ClearAll();

            var barcodeItemVM = new BarcodeItemViewModel(_mockNavigationService.Object, _damageStatuses, _barcodeScanningViewModel);

            _mockCustomUserInteraction.PopUpConfirmReturnsTrueIfTitleStartsWith("Mark Barcode as");

            barcodeItemVM.SelectBarcodeCommand.Execute(null);

            Assert.Equal(barcodeItemVM, _barcodeScanningViewModel.MarkAsProcessedBarcodeItem);
            Assert.False(_barcodeScanningViewModel.MarkAsProcessedWasScanned);

        }




        #endregion Test

        #region Helper classes

        public class TestBarcodeScanningModelVM : BarcodeScanningViewModel
        {
            public TestBarcodeScanningModelVM(INavigationService navigationService, IRepositories repositories) : base(navigationService, repositories) { }

            public override void MarkBarcodeAsProcessed(BarcodeItemViewModel barcodeItem, bool wasScanned = true)
            {

                MarkAsProcessedBarcodeItem = barcodeItem;
                MarkAsProcessedWasScanned = wasScanned;

                if (_unprocessedBarcodes.Contains(barcodeItem))
                    base.MarkBarcodeAsProcessed(barcodeItem);
            }

            public BarcodeItemViewModel MarkAsProcessedBarcodeItem
            {
                get;
                set;
            }

            public bool? MarkAsProcessedWasScanned
            {
                get;
                set;
            }

        }


        #endregion

    }
}
