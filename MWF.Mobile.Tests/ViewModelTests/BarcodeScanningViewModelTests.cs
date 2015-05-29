﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class BarcodeScanningViewModelTests
        : MvxIoCSupportingTest
    {
        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mockMobileDataRepo;
        private Mock<IVerbProfileRepository> _mockVerbProfileRepo;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IInfoService> _mockInfoService;
        private Mock<IMvxMessenger> _mockMessenger;
        private VerbProfile _verbProfile;


        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();
            _mobileData.Order.Type = InstructionType.Deliver;

            _mockMobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mockMobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _verbProfile = _fixture.Create<VerbProfile>();
            List<VerbProfile> verbProfiles = new List<VerbProfile>() { _verbProfile };
            _mockVerbProfileRepo = _fixture.InjectNewMock<IVerbProfileRepository>();
            _mockVerbProfileRepo.Setup(mvpr => mvpr.GetAll()).Returns(verbProfiles);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _mockNavigationService = _fixture.InjectNewMock<INavigationService>();

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            _mockInfoService = _fixture.InjectNewMock<IInfoService>();

            _fixture.Customize<BarcodeScanningViewModel>(vm => vm.Without(x => x.BarcodeSections));
            _fixture.Customize<BarcodeScanningViewModel>(vm => vm.Without(x => x.BarcodeInput));

            Ioc.RegisterSingleton<INavigationService>(_mockNavigationService.Object);
        }

        #endregion Setup

        #region Test

        [Fact]
        public void BarcodeScanningVM_FragmentTitle()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            Assert.Equal("Deliver Scan", barcodeScanningVM.FragmentTitle);

        }

        [Fact]
        // Checks that when the view model is initialised the sections are correctly constructed
        public void BarcodeScanningVM_Init_Sections()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // Check the section headers are corrrect
            Assert.Equal("To Do", barcodeScanningVM.BarcodeSections[0].SectionHeader);
            Assert.Equal("Done", barcodeScanningVM.BarcodeSections[1].SectionHeader);

            // Check the unprocessed section contains the barcodes specified on the delivery
            int totalBarcodesInOrder = _mobileData.Order.Items.Sum(x => x.BarcodesList.Count());
            Assert.Equal(totalBarcodesInOrder, barcodeScanningVM.BarcodeSections[0].Count());

            // Check the processed section contains no barcodes
            Assert.Equal(0, barcodeScanningVM.BarcodeSections[1].Count());

            // Check that the barcodes in the unprocessed section are correct
            var unprocessedBarcodes = barcodeScanningVM.BarcodeSections[0].Barcodes;
            var barcodesInOrder = _mobileData.Order.Items.SelectMany(x => x.BarcodesList).ToList();

            for (int i = 0; i < unprocessedBarcodes.Count; i++)
            {
                // barcode should match order
                Assert.Equal(barcodesInOrder[i], unprocessedBarcodes[i].BarcodeText);

                //should be not scannned, null delivery status, clean, no delivery comments
                Assert.False(unprocessedBarcodes[i].IsScanned);
                Assert.Null(unprocessedBarcodes[i].IsDelivered);
                Assert.Equal("Clean", unprocessedBarcodes[i].DamageStatus.Text);
                Assert.Equal("POD", unprocessedBarcodes[i].DamageStatus.Code);
                Assert.Equal(null, unprocessedBarcodes[i].DeliveryComments);
            }

            // no barcodes should be selected
            Assert.Equal(0, barcodeScanningVM.SelectedBarcodes.Count());


        }


        [Fact]
        // Checks that when the view model is initialised the damage statuses are correctly constructed
        public void BarcodeScanningVM_Init_DamageStatuses()
        {
            base.ClearAll();

            _verbProfile.Code = "PFORCE";

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            var barcodeItemViewModel = barcodeScanningVM.BarcodeSections[0].Barcodes[0];

            // check that an item view model has the right damage statuses
            // i.e. clean, plus whatever is specified under "PFORCE" verb profile
            Assert.Equal(_verbProfile.Children.Count + 1, barcodeItemViewModel.DamageStatuses.Count);

            Assert.Equal("Clean", barcodeItemViewModel.DamageStatuses[0].Text);
            Assert.Equal("POD", barcodeItemViewModel.DamageStatuses[0].Code);

            for (int i = 0; i < _verbProfile.Children.Count; i++)
            {
                Assert.Equal(_verbProfile.Children[i].Title, barcodeItemViewModel.DamageStatuses[i+1].Text);
                Assert.Equal(_verbProfile.Children[i].Category, barcodeItemViewModel.DamageStatuses[i + 1].Code);
            }

        }



        [Fact]
        public void BarcodeScanningVM_CanBeCompleted()
        {
            base.ClearAll();

            //change the order to only have one item with one barcode
            _mobileData.Order.Items.RemoveRange(1, _mobileData.Order.Items.Count - 1);

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // CanBeCompleted should be false to start with
            Assert.False(barcodeScanningVM.CanScanningBeCompleted);

            // Mark the only view model as processed
            var barcodeItemViewModel = barcodeScanningVM.BarcodeSections[0].Barcodes[0];
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeItemViewModel, wasScanned: true);

            // All processed, so CanBeCompleted should be true
            Assert.True(barcodeScanningVM.CanScanningBeCompleted);

        }


        [Fact]
        public void BarcodeScanningVM_CompleteScanning()
        {
            base.ClearAll();

            //change the order to only have one item with one barcode
            _mobileData.Order.Items.RemoveRange(1, _mobileData.Order.Items.Count - 1);

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();
            var navData = new NavData<MobileData>() { Data = _mobileData };
            barcodeScanningVM.Init(navData);

            // Mark the only view model as processed
            var barcodeItemViewModel = barcodeScanningVM.BarcodeSections[0].Barcodes[0];
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeItemViewModel, wasScanned: true);

            //set some other status values on the barcode
            barcodeItemViewModel.DeliveryComments = "Some comments.";
            barcodeScanningVM.CompleteScanningCommand.Execute(null);

            //check that the properties for the barcode were set correctly on the datachunk
            var dataChunk = navData.GetDataChunk();

            Assert.Equal(barcodeItemViewModel.BarcodeText, dataChunk.ScannedDelivery.Barcodes[0].BarcodeText);
            Assert.True(dataChunk.ScannedDelivery.Barcodes[0].IsDelivered);
            Assert.True(dataChunk.ScannedDelivery.Barcodes[0].IsScanned);
            Assert.Equal(barcodeItemViewModel.DeliveryComments, dataChunk.ScannedDelivery.Barcodes[0].DeliveryStatusNote);
            Assert.Equal(barcodeItemViewModel.DamageStatus.Code, dataChunk.ScannedDelivery.Barcodes[0].DamageStatusCode);
            Assert.Equal(barcodeItemViewModel.PalletforceDeliveryStatus, dataChunk.ScannedDelivery.Barcodes[0].DeliveryStatusCode);


            //check that the navigation service was called
            _mockNavigationService.Verify(ns => ns.MoveToNext(It.Is<NavData<MobileData>>(x => x == navData)));
            

        }


        [Fact]
        //Checks that the viewmodel can process changes to the barcode input property (i.e. when a barcode is scanned)
        public void BarcodeScanningVM_BarcodeInput()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // set the barcode input to be the same as the input barcode of the first view model
            var barcodeToInput = barcodeScanningVM.BarcodeSections[0].Barcodes[0];
            barcodeScanningVM.BarcodeInput = barcodeToInput.BarcodeText;

            // barcode should now be in the "processed" list
            Assert.True(barcodeScanningVM.BarcodeSections[1].Contains(barcodeToInput));

            // barcode should no longer be in the "unprocessed" list
            Assert.False(barcodeScanningVM.BarcodeSections[0].Contains(barcodeToInput));

            // barcode should be marked as scanned and delivered
            Assert.True(barcodeToInput.IsScanned);
            Assert.True(barcodeToInput.IsDelivered);

        }

        [Fact]
        //Checks that the viewmodel can process invalid changes to the barcode input property (i.e. when an unknown barcode is scanned)
        public void BarcodeScanningVM_BarcodeInput_InvalidCode()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // set the barcode input to be a nonsense code
            barcodeScanningVM.BarcodeInput = "nonsense1010110";

            // "processed" list should still be empty
            Assert.Equal(0, barcodeScanningVM.BarcodeSections[1].Count());

            // should have got an alert
            _mockUserInteraction.Verify(mui => mui.Alert(It.Is<string>(x=>x=="Invalid Barcode"), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [Fact]
        //Checks that the viewmodel deals with case when the barcode input is modified to indicate a barcode that has already been scanned
        public void BarcodeScanningVM_BarcodeInput_AlreadyScanned()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // set the barcode input to be the same as the input barcode of the first view model
            var barcodeToInput = barcodeScanningVM.BarcodeSections[0].Barcodes[0];
            barcodeScanningVM.BarcodeInput = barcodeToInput.BarcodeText;

            // do the same thing again
            barcodeScanningVM.BarcodeInput = barcodeToInput.BarcodeText;

            // should have got an alert
            _mockUserInteraction.Verify(mui => mui.Alert(It.Is<string>(x => x == "Barcode already scanned"), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // "processed" list should still have a single item empty
            Assert.Equal(1, barcodeScanningVM.BarcodeSections[1].Count());

        }


        [Fact]
        //Checks that the viewmodel can process manaual changes to the barcode input property (i.e. when a barcode is scanned)
        public void BarcodeScanningVM_MarkAsProcessed()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            _mockUserInteraction.ConfirmReturnsTrueIfTitleStartsWith("Mark Barcode as Manually Processed?");

            // set the barcode input to be the same as the input barcode of the first view model
            var barcodeToInput = barcodeScanningVM.BarcodeSections[0].Barcodes[0];
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeToInput, false);

            // barcode should now be in the "processed" list
            Assert.True(barcodeScanningVM.BarcodeSections[1].Contains(barcodeToInput));

            // barcode should no longer be in the "unprocessed" list
            Assert.False(barcodeScanningVM.BarcodeSections[0].Contains(barcodeToInput));

            // barcode should be marked delivered but not scanned
            Assert.False(barcodeToInput.IsScanned);
            Assert.True(barcodeToInput.IsDelivered);

        }


        [Fact]
        public void BarcodeScanningVM_SelectedBarcodes()
        {
            base.ClearAll();

            var barcodeScanningVM = _fixture.Create<BarcodeScanningViewModel>();

            barcodeScanningVM.Init(new NavData<MobileData>() { Data = _mobileData });

            // shouldn't have any selected barcodes to start with
            Assert.Equal(0, barcodeScanningVM.SelectedBarcodes.Count());

            // mark all three barcodes as processed (they need to be processed to be be counted as "selected")
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeScanningVM.BarcodeSections[0].Barcodes[0]);
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeScanningVM.BarcodeSections[0].Barcodes[0]);
            barcodeScanningVM.MarkBarcodeAsProcessed(barcodeScanningVM.BarcodeSections[0].Barcodes[0]);

            // select the first and third barcodes from the processed list
            barcodeScanningVM.BarcodeSections[1].Barcodes[0].IsSelected = true;
            barcodeScanningVM.BarcodeSections[1].Barcodes[2].IsSelected = true;

            // //check the "selected" list contains only the first and third barcodes
            Assert.Equal(2, barcodeScanningVM.SelectedBarcodes.Count());
            Assert.Contains(barcodeScanningVM.BarcodeSections[1].Barcodes[0], barcodeScanningVM.SelectedBarcodes);
            Assert.Contains(barcodeScanningVM.BarcodeSections[1].Barcodes[2], barcodeScanningVM.SelectedBarcodes);
        }

        #endregion Test

    }
}
