using Cirrious.MvvmCross.Plugins.PictureChooser;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class CameraViewModel : BaseFragmentViewModel
    {
        #region Private Members

        private MvxCommand _doneCommand;
        private MvxCommand _takePictureCommand;
        ObservableCollection<CameraImageViewModel> _imagesVM;
        List<Image> _images;
        private string _commentText;
        private readonly IMvxPictureChooserTask _pictureChooserTask;
        private IMainService _mainService;
        private INavigationService _navigationService;

        #endregion Private Members

        #region Construction

        public CameraViewModel(IMvxPictureChooserTask pictureChooserTask, IMainService mainService, INavigationService navigationService)
        {
            _pictureChooserTask = pictureChooserTask;
            _imagesVM = new ObservableCollection<CameraImageViewModel>();
            _mainService = mainService;
            _images = new List<Image>();
            _navigationService = navigationService;
        }

        #endregion Construction

        #region Public Properties

        public override string FragmentTitle
        {
            get { return "Camera"; }
        }

        public string DoneButtonLabel
        {
            get { return "Done"; }
        }

        public string TakePictureButtonLabel
        {
            get { return "Take Picture"; }
        }

        public string CommentHintText
        {
            get { return "Type Comment"; }
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
            get { return (_doneCommand = _doneCommand ?? new MvxCommand(() => DoDoneCommand())); }
        }

        public System.Windows.Input.ICommand TakePictureCommand
        {
            get { return (_takePictureCommand = _takePictureCommand ?? new MvxCommand(() => TakePicture())); }
        }

        public ObservableCollection<CameraImageViewModel> ImagesVM
        {
            get { return _imagesVM; }
            private set { _imagesVM = value; RaisePropertyChanged(() => ImagesVM); }
        }

        public List<Image> Images
        {
            get { return _images; }
            set { _images = value; RaisePropertyChanged(() => Images); }
        }

        #endregion Public Properties


        #region Private Methods

        private void DoDoneCommand()
        {
            //TODO: send photo to bluesphere
            _mainService.SendPhotoAndComment(CommentText, Images);
        }

        private void TakePicture()
        {
            // note use "TakePicture" for release
            // ChoosePictureFromLibrary should only be used when debugging with an emulator
            //_pictureChooserTask.ChoosePictureFromLibrary(400, 95, OnPictureTaken, () => { });
            _pictureChooserTask.TakePicture(400, 95, OnPictureTaken, () => { });

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
            Image image = new Image() { ID = Guid.NewGuid(), Sequence = sequenceNumber, Bytes = memoryStream.ToArray() };

            Images.Add(image);

            //Add to view model
            CameraImageViewModel imageViewModel = new CameraImageViewModel(image, this);
            ImagesVM.Add(imageViewModel);

        }

        /// <summary>
        /// Deletes an image from both the the view models and datamodels
        /// </summary>
        /// <param name="image"></param>
        internal void Delete(CameraImageViewModel image)
        {
            Images.Remove(image.Image);
            // remove view model
            ImagesVM.Remove(image);
        }

        #endregion Private Methods
    }
}
