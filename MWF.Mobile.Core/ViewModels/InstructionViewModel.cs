using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.ViewModels.Interfaces;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionViewModel :
        BaseInstructionNotificationViewModel,
        IBackButtonHandler,
        IVisible
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IMainService _mainService;
        private readonly IRepositories _repositories;
        private readonly IDataChunkService _dataChunkService;

        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private MvxCommand _progressInstructionCommand;
        private MvxCommand<Item> _showOrderCommand;
        private MvxCommand _editTrailerCommand;

        #endregion Private Fields

        #region Construction

        public InstructionViewModel(
            INavigationService navigationService,
            IRepositories repositories,
            IMainService mainService,
            IDataChunkService dataChunkService)
        {
            _navigationService = navigationService;
            _mainService = mainService;
            _repositories = repositories;
            _dataChunkService = dataChunkService;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = _navData.Data;

        }

        #endregion Construction

        #region Public Properties

        public string RunID { get { return _mobileData.Order.RouteTitle; } }

        public string ArriveDateTime { get { return _mobileData.Order.Arrive.ToStringIgnoreDefaultDate(); } }

        public string DepartDateTime { get { return _mobileData.Order.Depart.ToStringIgnoreDefaultDate(); } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string Notes
        {
            get
            {
                if (_mobileData.Order.Instructions == null || !_mobileData.Order.Instructions.Any()) return string.Empty;
                else return string.Join("\n", _mobileData.Order.Instructions.Select(i => i.Lines));
            }
        }

        public IList<Item> Orders { get { return _mobileData.Order.Items; } }

        public string TrailerReg { get { return (_mobileData.Order.Additional.Trailer == null) ? "No Trailer" : _mobileData.Order.Additional.Trailer.TrailerId; } }


        public bool ChangeTrailerAllowed
        {
            get
            {
                return _mobileData.Order.Additional.IsTrailerConfirmationEnabled &&
                      _mobileData.Order.Type == Enums.InstructionType.Collect &&
                      (_mobileData.ProgressState == Enums.InstructionProgress.NotStarted
                      || _mobileData.ProgressState == Enums.InstructionProgress.Driving);
            }
        }

        public string ArriveLabelText { get { return "Arrive"; } }

        public string DepartLabelText { get { return "Depart"; } }

        public string AddressLabelText { get { return "Address"; } }

        public string NotesLabelText { get { return "Notes"; } }

        public string OrdersLabelText { get { return "Orders"; } }

        public string TrailersLabelText { get { return "Trailer"; } }

        public string TrailerChangeButtonText { get { return "Change Trailer"; } }

        public string ProgressButtonText
        {
            get
            {

                string retVal;

                switch (_mobileData.ProgressState)
                {
                    case MWF.Mobile.Core.Enums.InstructionProgress.NotStarted:
                        retVal = "Drive";
                        break;
                    case MWF.Mobile.Core.Enums.InstructionProgress.Driving:
                        retVal = "On Site";
                        break;
                    case MWF.Mobile.Core.Enums.InstructionProgress.OnSite:
                        retVal = "On Site";
                        break;
                    case MWF.Mobile.Core.Enums.InstructionProgress.Complete:
                        retVal = string.Empty;
                        break;
                    default:
                        retVal = string.Empty;
                        break;
                }

                return retVal;
            }
        }

        public ICommand ProgressInstructionCommand
        {
            get
            {
                return (_progressInstructionCommand = _progressInstructionCommand ?? new MvxCommand(() => ProgressInstruction()));
            }
        }

        public ICommand ShowOrderCommand
        {
            get
            {
                return (_showOrderCommand = _showOrderCommand ?? new MvxCommand<Item>(v => ShowOrder(v)));
            }
        }

        public ICommand EditTrailerCommand
        {
            get
            {
                return (_editTrailerCommand = _editTrailerCommand ?? new MvxCommand(() => EditTrailer()));
            }
        }

        #endregion Public Properties

        #region Private Methods

        private void EditTrailer()
        {
            _navigationService.MoveToNext(_navData);
        }

        private void ProgressInstruction()
        {
            UpdateProgress();

            if (_mobileData.ProgressState == Enums.InstructionProgress.OnSite)
            {
                _navigationService.MoveToNext(_navData);
            }
        }

        private void UpdateProgress()
        {
            if (_mobileData.ProgressState == Enums.InstructionProgress.NotStarted)
            {
                _mobileData.ProgressState = Enums.InstructionProgress.Driving;
                _dataChunkService.SendDataChunk(_navData.GetDataChunk(), _mobileData, _mainService.CurrentDriver, _mainService.CurrentVehicle);
            }
            else if (_mobileData.ProgressState == Enums.InstructionProgress.Driving)
            {
                _mobileData.ProgressState = Enums.InstructionProgress.OnSite;
            }

            _repositories.MobileDataRepository.Update(_mobileData);

            RaisePropertyChanged(() => ProgressButtonText);
        }

        private void ShowOrder(Item order)
        {
            NavData<Item> navItem = new NavData<Item>() { Data = order };
            navItem.OtherData["MobileData"] = _mobileData;
            navItem.OtherData["DataChunk"] = _navData.GetDataChunk();
            _navigationService.MoveToNext(navItem);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString(); }
        }

        #endregion  BaseFragmentViewModel Overrides

        #region IBackButtonHandler Implementation

        public Task<bool> OnBackButtonPressed()
        {
            var task = new Task<bool>(() => false);

            //NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.GoBack();

            return task;
        }
        #endregion IBackButtonHandler Implementation

        #region BaseInstructionNotificationViewModel

        public override void CheckInstructionNotification(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Now refreshing the page.", () => GetMobileDataFromRepository(instructionID), "This instruction has been Updated", "OK");
                else
                    Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("Redirecting you back to the manifest screen", () => _navigationService.GoToManifest(), "This instruction has been Deleted");
            }
        }

        #endregion BaseInstructionNotificationViewModel

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
