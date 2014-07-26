using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using MWF.Mobile.Core.Services;


namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckFaultViewModel : BaseModalViewModel<bool>, IBackButtonHandler
    {

        #region Private Members

        private MvxCommand _doneCommand;
        private MvxCommand _takePictureCommand;
        private IStartupInfoService _startupInfoService;
        private readonly IMvxPictureChooserTask _pictureChooserTask;
        private SafetyCheckFault _safetyCheckFault;             // working copy of safety check fault for duration of this screen
        private SafetyCheckFault _originalSafetyCheckFault;     // original copy of safety check fault we'll write to when "done" is clicked
        private string _faultTypeText;
        ObservableCollection<SafetyCheckFaultImageViewModel> _images;

        #endregion

        #region Construction

        public SafetyCheckFaultViewModel(IStartupInfoService startupInfoService, IMvxPictureChooserTask pictureChooserTask)
        {
            _startupInfoService = startupInfoService;
            _pictureChooserTask = pictureChooserTask;
        }


        public void Init(SafetyCheckNavItem item)
        {
            base.Init(item.MessageID);

            // Get the safety check fault to display
            if (item.IsVehicle)
            {
                _originalSafetyCheckFault = _startupInfoService.CurrentVehicleSafetyCheckData.Faults.SingleOrDefault(f => f.ID == item.FaultID);
            }
            else
            {
                _originalSafetyCheckFault = _startupInfoService.CurrentTrailerSafetyCheckData.Faults.SingleOrDefault(f => f.ID == item.FaultID);
            }

            _faultTypeText = item.FaultTypeText;

            _safetyCheckFault = _originalSafetyCheckFault.Clone();

            PopulateImageList();
            
        }

        #endregion

        #region Public Properties

        public override string FragmentTitle
        {
            get { return "Log Fault"; }
        }

        public string DoneButtonLabel
        {
            get { return "Done"; }
        }

        public string TakePictureButtonLabel
        {
            get { return "Take Picture"; }
        }

        public string CheckTypeText
        {
            get { return _safetyCheckFault.Title;  }
        }

        public string CommentHintText
        {
            get { return "Type Comment"; }
        }

        public string CommentText
        {
            get { return _safetyCheckFault.Comment; }
            set 
            {
                _safetyCheckFault.Comment = value;
                RaisePropertyChanged(() => CommentText);
                RaisePropertyChanged(() => HasCommentText);
            }
        }

        public string InstructionsText
        {
            get { return "Add a comment and add pictures."; }
        }

        public bool HasCommentText
        {
            get { return !string.IsNullOrEmpty(_safetyCheckFault.Comment); }
        }

        public string DiscretionaryOrFailureText
        {
            get { return _faultTypeText; }
        }

        public System.Windows.Input.ICommand DoneCommand
        {
            get { return (_doneCommand = _doneCommand ?? new MvxCommand( () => DoDoneCommand())); }
        }

        public System.Windows.Input.ICommand TakePictureCommand
        {
            get { return (_takePictureCommand = _takePictureCommand ?? new MvxCommand(() => TakePicture() )); }
        }

        public ObservableCollection<SafetyCheckFaultImageViewModel> Images
        {
            get { return _images; }
            set { _images = value; RaisePropertyChanged(() => Images); }
        }

        #endregion

        #region Private Methods

        private void DoDoneCommand()
        {
            _originalSafetyCheckFault.ValuesFrom(_safetyCheckFault);
            ReturnResult(true);
        }

        private void TakePicture()
        {
            _pictureChooserTask.ChoosePictureFromLibrary(400, 95, OnPictureTaken, () => { });
            //_pictureChooserTask.TakePicture(400, 95, OnPictureTaken, () => { });

        }

        private void OnPictureTaken(Stream stream)
        {

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            int sequenceNumber = (Images.Any()) ? _images.Max(i => i.FaultImage.Sequence) : 1;
            Image image = new Image() { ID = Guid.NewGuid(), SafetyCheckFaultID = _safetyCheckFault.ID, Sequence = sequenceNumber, Bytes = memoryStream.ToArray() };


            //Add to data model
            _safetyCheckFault.Images.Add(image);

            //Add to view model
            SafetyCheckFaultImageViewModel imageViewModel = new SafetyCheckFaultImageViewModel(image, this);
            Images.Add(imageViewModel);

        }


        private void PopulateImageList()
        {
            ObservableCollection<SafetyCheckFaultImageViewModel> images = new ObservableCollection<SafetyCheckFaultImageViewModel>();

            foreach (var image in _safetyCheckFault.Images)
            {
                images.Add(new SafetyCheckFaultImageViewModel(image, this));
            }

            Images = images;
        }

        internal void Delete(SafetyCheckFaultImageViewModel image)
        {
            // remove from backing data model (exists in startup service)
            _safetyCheckFault.Images.Remove(image.FaultImage);

            // remove view model
            Images.Remove(image);
        }

        #endregion


        #region IBackButtonHandler Implementation

        public async Task<bool> OnBackButtonPressed()
        {
            bool continueWithBackPress = await Mvx.Resolve<IUserInteraction>().ConfirmAsync("All changes will be lost, do you wish to continue?", "Changes will be lost!");

            // since we are modal, we need to let the calling viewmodel know that we cancelled (it will handle the back press)
            if (continueWithBackPress)
            {
                this.Cancel();
            }

            return false;
        }

        #endregion
    }


}
