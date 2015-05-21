using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
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

        public string TrailerReg
        {
            get
            {
                return string.Format("{0} {1}", OrderTrailerReg, CurrentTrailerReg);
            }
        }

        public string OrderTrailerReg 
        { 
            get 
            { 
                return (_mobileData.Order.Additional.Trailer == null) ? "No trailer" : _mobileData.Order.Additional.Trailer.TrailerId; 
            } 
        }

        public string CurrentTrailerReg 
        { 
            get 
            {
                var reg = (_mainService.CurrentTrailer == null) ? "No trailer" : _mainService.CurrentTrailer.Registration;
                if (reg == OrderTrailerReg) return string.Empty;

                return string.Format("(Current: {0})", reg);
            } 
        }


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
            _navData.OtherData["IsTrailerEditFromInstructionScreen"] = true;
            _navigationService.MoveToNext(_navData);
        }

        private void ProgressInstruction()
        {
            UpdateProgress();

            if (_mobileData.ProgressState == Enums.InstructionProgress.OnSite)
            {
                _navData.OtherData["IsTrailerEditFromInstructionScreen"] = null;
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

            NavData<MobileData> navData = new NavData<MobileData>();

            navData.Data = _mobileData;
            navData.OtherData["DataChunk"] = navData.GetDataChunk();

            navData.OtherData["Order"] = order;

            _navigationService.ShowModalViewModel<OrderViewModel, bool>(this, navData, (modified) =>
            {
                if (modified)
                {

                }
            }
            );

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
            _navigationService.GoBack();
            return Task.FromResult(false);
        }

        #endregion IBackButtonHandler Implementation

        #region BaseInstructionNotificationViewModel

        public override async Task CheckInstructionNotificationAsync(GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            if (instructionID == _mobileData.ID)
            {
                if (notificationType == GatewayInstructionNotificationMessage.NotificationCommand.Update)
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Now refreshing the page.", "This instruction has been updated");
                    GetMobileDataFromRepository(instructionID);
                }
                else
                {
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Redirecting you back to the manifest screen", "This instruction has been deleted");
                    _navigationService.GoToManifest();
                }
            }
        }

        #endregion BaseInstructionNotificationViewModel

        #region IVisible

        public void IsVisible(bool isVisible)
        {
            if (isVisible) 
            {

            }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        #endregion IVisible
    }
}
