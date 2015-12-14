using Cirrious.CrossCore;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionTrunkProceedViewModel
        : BaseInstructionNotificationViewModel
    {
        #region Private Members

        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IInfoService _infoService;
        private IRepositories _repositories;
        private INavigationService _navigationService;

        private MvxCommand _completeInstructionCommand;

        #endregion Private Members

        #region Construction

        public InstructionTrunkProceedViewModel(INavigationService navigationService, IRepositories repositories, IInfoService infoService)
        {
            _navigationService = navigationService;
            _infoService = infoService;
            _repositories = repositories;
        }

        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            navData.Reinflate();
            _mobileData = navData.Data;
        }

        #endregion Construction

        #region Public Properties

        public string RunID { get { return _mobileData.Order.RouteTitle; } }

        public string ArriveDepartDateTime { get { return _mobileData.Order.Arrive.ToStringIgnoreDefaultDate(); } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string ArriveDepartLabelText { get { return (IsTrunkTo) ? "Arrive" : "Depart"; } }

        public string AddressLabelText { get { return "Address"; } }

        public string ProgressButtonText { get { return "Complete"; } }

        public bool IsTrunkTo { get { return _mobileData.Order.Type == Enums.InstructionType.TrunkTo; } }

        public ICommand CompleteInstructionCommand
        {
            get
            {
                return (_completeInstructionCommand = _completeInstructionCommand ?? new MvxCommand(async () => await this.CompleteInstructionAsync()));
            }
        }

        #endregion Public Properties

        #region Private Methods

        private Task CompleteInstructionAsync()
        {
            return _navigationService.MoveToNextAsync(_navData);
        }

        private async Task RefreshPageAsync(Guid ID)
        {
            _mobileData = await _repositories.MobileDataRepository.GetByIDAsync(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods

        #region BaseInstructionNotificationViewModel

        public override async Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    if (this.IsVisible) 
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated");

                    await this.RefreshPageAsync(instructionID);
                }
                else
                {
                    if (this.IsVisible)
                    {
                        await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted.");
                        await _navigationService.GoToManifestAsync();
                    }
                }
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle { get { return (IsTrunkTo) ? "Trunk To" : "Proceed From"; } }

        #endregion  BaseFragmentViewModel Overrides


    }
}
