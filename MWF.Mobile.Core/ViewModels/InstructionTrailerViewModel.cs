using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionTrailerViewModel
        : BaseTrailerListViewModel,
        IVisible
    {
        #region Private Fields


        private MobileData _mobileData;
        private IMainService _mainService;
        private NavData<MobileData> _navData;


        private MvxSubscriptionToken _notificationToken;
        private IMvxMessenger _messenger;


        #endregion

        #region Construction

        public InstructionTrailerViewModel(IGatewayService gatewayService,
                                            INavigationService navigationService, 
                                            IRepositories repositories,
                                            IReachability reachabiity,
                                            IToast toast,
                                            IStartupService startUpService,
                                            IMainService mainService) : base(gatewayService, repositories, reachabiity, toast, startUpService, navigationService )
        {
            _mainService = mainService;

            _notificationToken = Messenger.Subscribe<Messages.GatewayInstructionNotificationMessage>(m =>
                CheckInstructionNotification(m.Command, m.InstructionID)
                );
        }

        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();
            _mobileData = navData.Data;
        }


        #endregion

        #region Private Properties

        protected new IMvxMessenger Messenger
        {
            get
            {
                return (_messenger = _messenger ?? Mvx.Resolve<IMvxMessenger>());
            }
        }

        private bool IsInstructionInProgress
        {
            get { return _mobileData.ProgressState != Enums.InstructionProgress.NotStarted; }
        }

        private void UnsubscribeNotificationToken()
        {
            if (_notificationToken != null)
                Messenger.Unsubscribe<Messages.GatewayInstructionNotificationMessage>(_notificationToken);
        }

        #endregion

        #region Protected/Private Methods

        protected override void ConfirmTrailer(Models.Trailer trailer, string title, string message)
        {

            //This will take to the next view model with a trailer value of null.
            Mvx.Resolve<ICustomUserInteraction>().PopUpConfirm(message, async isConfirmed =>
            {
                if (isConfirmed)
                {
                    // if a trailer has been selected it differs from the current trailer then we need to update
                    // everything requirede to update safety profiles is readiness for the next step
                    if (trailer!=null && trailer.ID != _mainService.CurrentTrailer.ID)
                    {
                          //This will take to the next view model with a trailer value of null.
                        Mvx.Resolve<ICustomUserInteraction>().PopUpConfirm("Perform a safety check for the new trailer now?", async isConfirmedUpdate =>
                        {
                            this.IsBusy = true;

                            await UpdateVehicleListAsync();

                            await UpdateTrailerListAsync();
                            // Try and update safety profiles before continuing
                            await UpdateSafetyProfilesAsync();

                            this.IsBusy = false;
                        }, "Perform Safety Check");
                   
                    }

                    _navigationService.MoveToNext(_navData);

                }
            }, title, "Confirm");

        
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();

        }

        private void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion

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
