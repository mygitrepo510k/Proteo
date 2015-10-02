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
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionTrailerViewModel
        : BaseTrailerListViewModel, IInstructionNotificationViewModel
    {
        #region Private Fields


        private MobileData _mobileData;
        private IInfoService _infoService;
        private NavData<MobileData> _navData;


        private MvxSubscriptionToken _notificationToken;
        private IMvxMessenger _messenger;

        private readonly IApplicationProfileRepository _applicationProfileRepository;

        #endregion

        #region Construction

        public InstructionTrailerViewModel(IGatewayService gatewayService,
                                            INavigationService navigationService, 
                                            IRepositories repositories,
                                            IReachability reachabiity,
                                            IToast toast,
                                            IInfoService startUpService,
                                            IInfoService infoService) : base(gatewayService, repositories, reachabiity, toast, startUpService, navigationService )
        {
            _infoService = infoService;

            _notificationToken = Messenger.Subscribe<Messages.GatewayInstructionNotificationMessage>(async m => await CheckInstructionNotificationAsync(m.Command, m.InstructionID));
            _applicationProfileRepository = _repositories.ApplicationRepository;
        }

        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();
            _mobileData = navData.Data;

            //set the default trailer to be the one specified on the order
            if (_mobileData.Order.Additional.Trailer != null)
            {
                this.DefaultTrailerReg = _mobileData.Order.Additional.Trailer.TrailerId;
            }
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

        #endregion

        #region Protected/Private Methods

        protected override async Task ConfirmTrailerAsync(Models.Trailer trailer, string title, string message)
        {
            if (await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(message, title, "Confirm"))
            {
                await UpdateReadyForSafetyCheck(trailer);
                _navData.OtherData["UpdatedTrailer"] = trailer;
                _navigationService.MoveToNext(_navData);
            }
        }

        private async Task UpdateReadyForSafetyCheck(Models.Trailer trailer)
        {
            // if a trailer has been selected it differs from the current trailer then we need to update
            // everything requirede to update safety profiles is readiness for the next step
            if (trailer != null && (!Models.Trailer.SameAs(trailer, _infoService.CurrentTrailer)))
            {
                this.IsBusy = true;

                try
                {
                    // we only need to check profiles once a day
                    var applicationProfile = _applicationProfileRepository.GetAll().First();
                    if (DateTime.Now.Subtract(applicationProfile.LastVehicleAndDriverSync).TotalHours > 23)
                    {
                        await UpdateVehicleListAsync();
                        await UpdateTrailerListAsync();

                        // Try and update safety profiles before continuing
                        await UpdateSafetyProfilesAsync();

                        applicationProfile.LastVehicleAndDriverSync = DateTime.Now;
                        _applicationProfileRepository.Update(applicationProfile);
                    }
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();

        }

        #endregion

        #region Core.Portable.IDisposable

        public void Dispose()
        {
            UnsubscribeNotificationToken();
        }

        #endregion

        #region IInstructionNotificationViewModel

        public void UnsubscribeNotificationToken()
        {
            if (_notificationToken != null)
                Messenger.Unsubscribe<Messages.GatewayInstructionNotificationMessage>(_notificationToken);
        }

        public async Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    if (this.IsVisible)
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated");
                    GetMobileDataFromRepository(instructionID);
                }
                else
                {
                    if (this.IsVisible)
                    {
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                        _navigationService.GoToManifest();
                    }
                }
            }
        }

        #endregion

    }
}
