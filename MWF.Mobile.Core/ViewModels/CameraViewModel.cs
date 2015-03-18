using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;


namespace MWF.Mobile.Core.ViewModels
{
    public class CameraViewModel : BaseInstructionNotificationViewModel, IBackButtonHandler
    {
        #region Private Members

        private MvxCommand _doneCommand;
        private MvxCommand _takePictureCommand;
        private MvxCommand _selectPictureFromLibrary;
        ObservableCollection<CameraImageViewModel> _imagesVM;
        List<Image> _images;
        private string _commentText;
        private readonly IMvxPictureChooserTask _pictureChooserTask;
        private IMainService _mainService;
        private INavigationService _navigationService;
        private IImageUploadService _imageUploadService;

        #endregion Private Members

        #region Construction

        public CameraViewModel(
            IMvxPictureChooserTask pictureChooserTask, 
            IMainService mainService, 
            INavigationService navigationService,
            IImageUploadService imageUploadService)
        {
            _pictureChooserTask = pictureChooserTask;
            _imagesVM = new ObservableCollection<CameraImageViewModel>();
            _mainService = mainService;
            _images = new List<Image>();
            _navigationService = navigationService;
            _imageUploadService = imageUploadService;
        }

        #endregion Construction

        #region Public Properties

        public string DoneButtonLabel
        {
            get { return "Done"; }
        }

        public string TakePictureButtonLabel
        {
            get { return "Take Picture"; }
        }

        public string SelectFromLibraryButtonLabel
        {
            get { return "Select From Library"; }
        }

        public string CommentHintText
        {
            get
            {
                return (HasPhotoBeenTaken) ? "Type Comment" : "Take a photo to enter a comment";
            }
        }

        public string InstructionsText
        {
            get { return "Add comment and images"; }
        }

        public string CommentText
        {
            get { return _commentText; }
            set { _commentText = value; RaisePropertyChanged(() => CommentText); }
        }

        public System.Windows.Input.ICommand DoneCommand
        {
            get { return (_doneCommand = _doneCommand ?? new MvxCommand(async () => await DoDoneCommand())); }
        }

        public System.Windows.Input.ICommand TakePictureCommand
        {
            get { return (_takePictureCommand = _takePictureCommand ?? new MvxCommand(() => TakePicture())); }
        }

        public System.Windows.Input.ICommand SelectPictureFromLibraryCommand
        {
            get { return (_selectPictureFromLibrary = _selectPictureFromLibrary ?? new MvxCommand(() => SelectPictureFromLibrary()));}
        }

        public ObservableCollection<CameraImageViewModel> ImagesVM
        {
            get { return _imagesVM; }
            private set
            {
                _imagesVM = value; RaisePropertyChanged(() => ImagesVM);
            }
        }

        public bool HasPhotoBeenTaken
        {
            get
            {
                return (ImagesVM.Count > 0) ? true : false;
            }
        }

        #endregion Public Properties

        #region Private Methods

        private async Task DoDoneCommand()
        {
            List<Image> images = new List<Image>();

            foreach (var viewModel in ImagesVM)
            {
                images.Add(viewModel.Image);
            }
            _navigationService.MoveToNext(_navigationService.CurrentNavData);

            MobileData mobileData = null; ;
            if (_navigationService.CurrentNavData != null) {
                mobileData = _navigationService.CurrentNavData.GetMobileData();
            }

            await _imageUploadService.SendPhotoAndCommentAsync(CommentText, images, _mainService.CurrentDriver, mobileData);
        }

        private void TakePicture()
        {
            // note use "TakePicture" for release
            // ChoosePictureFromLibrary should only be used when debugging with an emulator
            //_pictureChooserTask.ChoosePictureFromLibrary(400, 95, OnPictureTaken, () => { });
            _pictureChooserTask.TakePicture(400, 95, OnPictureTaken, () => { });       
        }

        private void SelectPictureFromLibrary()
        {
            _pictureChooserTask.ChoosePictureFromLibrary(400, 95, OnPictureTaken, () => { });
        }


        /// <summary>
        /// Packs up the image stre
        /// </summary>
        /// <param name="stream"></param>
        private void OnPictureTaken(Stream stream)
        {

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            int sequenceNumber = (ImagesVM.Any()) ? _imagesVM.Max(i => i.Image.Sequence) + 1 : 1;
            Image image = new Image() { ID = Guid.NewGuid(), Sequence = sequenceNumber, Bytes = memoryStream.ToArray(), Filename = string.Format("{0} {1}.jpg", _mainService.CurrentDriver.DisplayName, DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss")) };

            //Add to view model
            CameraImageViewModel imageViewModel = new CameraImageViewModel(image, this);
            ImagesVM.Add(imageViewModel);

            RaisePropertyChanged(() => HasPhotoBeenTaken);
            RaisePropertyChanged(() => CommentHintText);

        }

        /// <summary>
        /// Deletes an image from both the the view models and datamodels
        /// </summary>
        /// <param name="image"></param>
        internal void Delete(CameraImageViewModel image)
        {
            // remove view model
            ImagesVM.Remove(image);

            RaisePropertyChanged(() => HasPhotoBeenTaken);
            RaisePropertyChanged(() => CommentHintText);
        }

        #endregion Private Methods

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            var task = new Task<bool>(() => false);

            _navigationService.GoBack(_navigationService.CurrentNavData);

            return task;
        }
        #endregion IBackButtonHandler Implementation

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (_navigationService.CurrentNavData != null && _navigationService.CurrentNavData.GetMobileData() != null && _navigationService.CurrentNavData.GetMobileData().ID == instructionID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Data may have changed.", null, "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Camera"; }
        }

        #endregion BaseFragmentViewModel Overrides
    }
}
