using Cirrious.MvvmCross.Plugins.PictureChooser;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class ModalCameraViewModel
        : BaseCameraViewModel
        , IModalViewModel<bool>,
        IBackButtonHandler
    {


        #region private members


        private MobileData _mobileData;
        private NavData<MobileData> _navData;

        #endregion

        #region construction

        public ModalCameraViewModel(
            IMvxPictureChooserTask pictureChooserTask, 
            IMainService mainService, 
            INavigationService navigationService,
            IImageUploadService imageUploadService)
            : base(pictureChooserTask, mainService, navigationService, imageUploadService)
        {

        }

        #endregion

        protected override async Task DoDoneCommand()
        {
            List<Image> images = new List<Image>();

            foreach (var viewModel in ImagesVM)
            {
                images.Add(viewModel.Image);
            }

            this.ReturnResult(true);

            IEnumerable<MobileData> mobileDatas = null;
            if (_navigationService.CurrentNavData != null && _navigationService.CurrentNavData is NavData<MobileData>)
            {
                mobileDatas = (_navigationService.CurrentNavData as NavData<MobileData>).GetAllInstructions();
            }

            await _imageUploadService.SendPhotoAndCommentAsync(CommentText, images, _mainService.CurrentDriver, mobileDatas);
        }


        #region IModalViewModel

        public Guid MessageId { get; set; }  

        public void Cancel()
        {
            ReturnResult(default(bool));
        }

        public void ReturnResult(bool result)
        {
            var message = new ModalNavigationResultMessage<bool>(this, MessageId, result);

            this.Messenger.Publish(message);
            this.Close(this);
        }

        #endregion

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            this.Cancel();
            return Task.FromResult(false);
        }

        #endregion IBackButtonHandler Implementation
    }
}
