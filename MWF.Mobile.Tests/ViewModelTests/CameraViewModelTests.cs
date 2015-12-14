﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class CameraViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<INavigationService> _navigationService;
        private Mock<IMvxPictureChooserTask> _pictureChooserMock;
        private Mock<IInfoService> _mockInfoService;
        private Mock<ICustomUserInteraction> _mockUserInteraction;
        private Mock<IImageUploadService> _mockImageUploadService;
        private Mock<IMvxMessenger> _mockMessenger;

        private byte[] _pictureBytes;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _pictureBytes = new byte[] { 1, 2, 3, 4 };
            _pictureChooserMock = new Mock<IMvxPictureChooserTask>();
            _pictureChooserMock.Setup(pc => pc.TakePicture(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Action<Stream>>(), It.IsAny<Action>())).
                                Callback<int, int, Action<Stream>, Action>((s1, s2, a1, a2) => { a1.Invoke(new MemoryStream(_pictureBytes)); });
            _fixture.Inject<IMvxPictureChooserTask>(_pictureChooserMock.Object);

            _navigationService = _fixture.InjectNewMock<INavigationService>();

            _mockInfoService = _fixture.InjectNewMock<IInfoService>();
            _mockInfoService.Setup(m => m.LoggedInDriver).Returns(_fixture.Create<Driver>());

            _mockUserInteraction = Ioc.RegisterNewMock<ICustomUserInteraction>();
            _mockUserInteraction.ConfirmReturnsTrue();

            _mockMessenger = Ioc.RegisterNewMock<IMvxMessenger>();
            _mockMessenger.Setup(m => m.Unsubscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<MvxSubscriptionToken>()));
            _mockMessenger.Setup(m => m.Subscribe<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>(It.IsAny<Action<MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage>>(), It.IsAny<MvxReference>(), It.IsAny<string>())).Returns(_fixture.Create<MvxSubscriptionToken>());

            _mockImageUploadService = _fixture.InjectNewMock<IImageUploadService>();


        }

        #endregion Setup

        #region Tests

        [Fact]
        public void CommentHintText_Enabled()
        {
            base.ClearAll();

            var cameraVM = _fixture.Create<SidebarCameraViewModel>();

            cameraVM.TakePictureCommand.Execute(null);

            Assert.Equal("Type Comment", cameraVM.CommentHintText);
        }

        [Fact]
        public void CommentHintText_Disabled()
        {
            base.ClearAll();

            var cameraVM = _fixture.Create<SidebarCameraViewModel>();

            Assert.Equal("Take a photo to enter a comment", cameraVM.CommentHintText);
        }

        [Fact]
        public void CommentHintText_TakePicture()
        {
            base.ClearAll();

            var cameraVM = _fixture.Build<SidebarCameraViewModel>().Without(p => p.CommentText).Create<SidebarCameraViewModel>();

            int previousImageCount = cameraVM.ImagesVM.Count;

            // Take the picture
            cameraVM.TakePictureCommand.Execute(null);

            // should have an extra image in view model model
            Assert.Equal(previousImageCount + 1, cameraVM.ImagesVM.Count);

            Assert.Equal(true, cameraVM.HasPhotoBeenTaken);

            //// Invoke the done command
            cameraVM.DoneCommand.Execute(null);

            _navigationService.Verify(ns => ns.MoveToNextAsync(It.IsAny<NavData<MobileData>>()), Times.Once);
            _mockImageUploadService.Verify(mis => mis.SendPhotoAndCommentAsync(It.IsAny<string>(), It.IsAny<List<Image>>(), It.IsAny<Driver>(), It.IsAny<List<MobileData>>()), Times.Once);

        }

        // Checks that when the delete command is executed on a fault image the image is deleted from the view model (initially) and the data
        // model (when done has been pressed)
        [Fact]
        public void CameraVM_Delete_OK()
        {

            base.ClearAll();

            var cameraVM = _fixture.Build<SidebarCameraViewModel>().Without(p => p.CommentText).Create<SidebarCameraViewModel>();

            CameraImageViewModel cameraImageVM = new CameraImageViewModel(_fixture.Create<Image>(), cameraVM);
            cameraVM.ImagesVM.Add(cameraImageVM);

            int previousImageCount = cameraVM.ImagesVM.Count;

            // dialog returns true
            _mockUserInteraction.ConfirmAsyncReturnsTrueIfTitleStartsWith("Delete Picture");

            //delete the last image
            cameraVM.ImagesVM[previousImageCount - 1].DeleteCommand.Execute(null);

            // should have one less image in view model 
            Assert.Equal(previousImageCount - 1, cameraVM.ImagesVM.Count);

        }

        [Fact]
        // Checks that when the user cancels out of an image deletion nothing is actually deleted
        public void CameraVM_Delete_Cancel()
        {

            base.ClearAll();

            var cameraVM = _fixture.Build<SidebarCameraViewModel>().Without(p => p.CommentText).Create<SidebarCameraViewModel>();

            CameraImageViewModel cameraImageVM = new CameraImageViewModel(_fixture.Create<Image>(), cameraVM);
            cameraVM.ImagesVM.Add(cameraImageVM);

            int previousImageCount = cameraVM.ImagesVM.Count;

            // dialog returns false
            _mockUserInteraction.ConfirmReturnsFalse();

            // attempt to delete the last image
            cameraVM.ImagesVM[previousImageCount - 1].DeleteCommand.Execute(null);

            // nothing should have been deleted since we cancelled out of the deletion
            Assert.Equal(previousImageCount, cameraVM.ImagesVM.Count);


        }

        [Fact]
        // Checks that when the Display command is executed on an image the image is displayed in a popup image
        public void CameraVM_DisplayImage()
        {

            base.ClearAll();

            var customUI = new Mock<ICustomUserInteraction>();
            Ioc.RegisterSingleton<ICustomUserInteraction>(customUI.Object);

            var CameraVM = _fixture.Build<SidebarCameraViewModel>().Without(p => p.CommentText).Create<SidebarCameraViewModel>();

            CameraVM.TakePictureCommand.Execute(null);

            CameraVM.ImagesVM[0].DisplayCommand.Execute(null);

            customUI.Verify(cui => cui.PopUpImage(It.Is<byte[]>(ba => ba[0] == CameraVM.ImagesVM[0].Bytes[0]),
                                                  It.IsAny<string>(),
                                                  It.IsAny<Action>(),
                                                  It.IsAny<string>(),
                                                  It.Is<string>(s => s == "Close")));
        }


        [Fact]
        public void CameraVM_CheckInstructionNotification_Delete()
        {

            base.ClearAll();

            _mockUserInteraction.Setup(cui => cui.Alert(It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            _navigationService.SetupGet( x=> x.CurrentNavData).Returns(new NavData<MobileData>() { Data = _mobileData});

            var cameraVM = _fixture.Create<SidebarCameraViewModel>();

            cameraVM.IsVisible = true;

            cameraVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            _navigationService.Verify(ns => ns.GoToManifestAsync(), Times.Once);

        }


        [Fact]
        public void CameraVM_CheckInstructionNotification_Update_Confirm()
        {

            base.ClearAll();

            var cameraVM = _fixture.Create<SidebarCameraViewModel>();

            cameraVM.IsVisible = true;

            _navigationService.SetupGet(x => x.CurrentNavData).Returns(new NavData<MobileData>() { Data = _mobileData });

            cameraVM.CheckInstructionNotificationAsync(Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, _mobileData.ID);

            _mockUserInteraction.Verify(cui => cui.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }


        #endregion Tests

    }
}
