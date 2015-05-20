using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using MWF.Mobile.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionClausedViewModel
        : BaseInstructionNotificationViewModel,
        IVisible
    {

        #region Private Properties

        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;

        #endregion Private Properties

        #region Construction

        public InstructionClausedViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
        }

        #endregion Construction

        #region Public Properties

        public string PhoneSentenceText
        {
            get
            {
                return "Please call the office to confirm details";
            }
        }

        public string PhotoSentenceText
        {
            get
            {
                return "Use the photo button to take one or more photos of the affected delivery";
            }
        }

        public string AdvanceButtonText
        {
            get
            {
                return "Continue";
            }
        }

        private MvxCommand _openCameraScreenCommand;
        public ICommand OpenCameraScreenCommand
        {
            get
            {
                return (_openCameraScreenCommand = _openCameraScreenCommand ?? new MvxCommand(() => OpenCameraScreen()));
            }
        }

        private MvxCommand _launchPhoneAppCommand;
        public ICommand LaunchPhoneAppCommand
        {
            get
            {
                return (_launchPhoneAppCommand = _launchPhoneAppCommand ?? new MvxCommand(() => LaunchPhoneApp()));
            }
        }

        private MvxCommand _advanceInstructionCommand;
        public ICommand AdvanceInstructionCommand
        {
            get
            {
                return (_advanceInstructionCommand = _advanceInstructionCommand ?? new MvxCommand(() => AdvanceInstruction()));
            }
        }

        #endregion Public Properties

        #region Private Methods

        private void AdvanceInstruction()
        {
            var dataChunks = _navData.GetAllDataChunks();
            foreach (var datachunk in dataChunks)
            {
                datachunk.IsClaused = true;
            }

            _navigationService.MoveToNext(_navData);
        }

        private void LaunchPhoneApp()
        {
            Mvx.Resolve<ILaunchPhone>().Launch();
        }

        private void OpenCameraScreen()
        {

            var modal = _navigationService.ShowModalViewModel<ModalCameraViewModel, bool>(this, _navData, (sendChunk) => {});
        }

        private void RefreshPage(Guid ID)
        {
            _navData.ReloadInstruction(ID, _repositories);
            _mobileData = _navData.Data;
            RaiseAllPropertiesChanged();
        }


        #endregion Private Methods

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (_navData.GetAllInstructions().Any(i => i.ID == instructionID))
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Now refreshing the page.", () => RefreshPage(instructionID), "This instruction has been updated.", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Claused Delivery"; }
        }

        #endregion BaseFragmentViewModel Overrides

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            _navigationService.GoBack(_navData);
            return Task.FromResult(false);
        }

        #endregion IBackButtonHandler Implementation

        #region IVisible

        public void IsVisible(bool isVisible)
        {
            if (isVisible) { }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        #endregion IVisible
    }
}
