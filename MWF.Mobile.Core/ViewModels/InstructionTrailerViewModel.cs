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

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionTrailerViewModel
        : BaseInstructionNotificationViewModel
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private IEnumerable<Models.Trailer> _trailers;
        private MvxCommand _selectTrailerCommand;
        private IMainService _mainService;


        #endregion

        #region Construction

        public InstructionTrailerViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _mainService = mainService;
            _repositories = repositories;
            Trailers = _repositories.TrailerRepository.GetAll();

        }

        public void Init(NavItem<MobileData> item)
        {
            GetMobileDataFromRepository(item.ID);
        }

        public void Init(NavItem<Models.Instruction.Trailer> item)
        {
            GetMobileDataFromRepository(item.ID);
        }


        #endregion

        #region Public Properties

        
        public IEnumerable<Models.Trailer> Trailers
        {
            get { return _trailers; }
            set { _trailers = value; RaisePropertyChanged(() => Trailers); }
        }

        public ICommand SelectTrailerCommand
        {
            get
            {
                return (_selectTrailerCommand = _selectTrailerCommand ?? new MvxCommand(() => SelectTrailer()));
            }
        }

        public string InstructionTrailerButtonLabel { get { return "Move on"; } }

        #endregion

        #region Private Methods

        private void SelectTrailer()
        {
            if(_mobileData.ProgressState == Enums.InstructionProgress.NotStarted)
            {
                NavItem<Models.Instruction.Trailer> navItem = new NavItem<Models.Instruction.Trailer>() { ID = _mobileData.ID };
                _navigationService.MoveToNext(navItem);
            }
            else if(_mobileData.ProgressState == Enums.InstructionProgress.OnSite)
            {
                NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
                _navigationService.MoveToNext(navItem);
            }            
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            RaiseAllPropertiesChanged();
            _mainService.CurrentMobileData = _mobileData;
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Trailer"; }
        }

        #endregion

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mainService.CurrentMobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpCurrentInstructionNotifaction("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

    }
}
