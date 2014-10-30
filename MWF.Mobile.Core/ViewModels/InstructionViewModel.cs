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

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IMobileApplicationDataChunkService _mobileApplicationDataChunkService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private MvxCommand _progressInstructionCommand;
        private MvxCommand<Item> _showOrderCommand;
        private MvxCommand _editTrailerCommand;

        #endregion

        #region Construction

        public InstructionViewModel(INavigationService navigationService, IRepositories repositories, IMobileApplicationDataChunkService mobileApplicationDataChunkService)
        {
            _navigationService = navigationService;
            _mobileApplicationDataChunkService = mobileApplicationDataChunkService;
            _repositories = repositories;
        }

        public void Init(NavItem<MobileData> item)
        {        
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
        }

        #endregion

        #region Public Properties

        public string RunID { get { return _mobileData.GroupTitleFormatted; } }

        public string ArriveDateTime { get { return _mobileData.Order.Arrive.ToStringIgnoreDefaultDate(); } }

        public string DepartDateTime { get { return _mobileData.Order.Depart.ToStringIgnoreDefaultDate(); } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|","\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string Notes 
        { 
            get 
            {
                if (_mobileData.Order.Instructions == null || !_mobileData.Order.Instructions.Any()) return string.Empty;
                else return string.Join("\n", _mobileData.Order.Instructions.Select(i => i.Lines));
            }
        }

        public IList<Item> Orders { get { return _mobileData.Order.Items; } }

        public string TrailerReg { get { return (_mobileData.Order.Additional.Trailer == null) ? "No Trailer" : _mobileData.Order.Additional.Trailer.DisplayName; } }


        public bool ChangeTrailerAllowed 
        { 
            get 
            {
                return _mobileData.Order.Additional.IsTrailerConfirmationEnabled &&
                      _mobileData.Order.Type == Enums.InstructionType.Collect &&
                      _mobileData.ProgressState == Enums.InstructionProgress.NotStarted;
            } 
        }

        public string ArriveLabelText { get { return "Arrive"; } }

        public string DepartLabelText { get { return "Depart"; } }

        public string AddressLabelText { get { return "Address"; } }

        public string NotesLabelText { get { return "Notes"; } }

        public string OrdersLabelText { get { return "Orders"; } }

        public string TrailersLabelText { get { return "Trailer"; } }

        public string TrailerChangeButtonText { get { return "Change Trailer";  } }

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
                return(_editTrailerCommand = _editTrailerCommand ?? new MvxCommand(() => EditTrailer()));
            }
        }

        #endregion

        #region Private Methods

        private void EditTrailer()
        {
            NavItem<Trailer> navItem = new NavItem<Trailer>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        private void ProgressInstruction()
        {
            UpdateProgress();

            if (_mobileData.ProgressState == Enums.InstructionProgress.OnSite)
            {
                var navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
                _navigationService.MoveToNext(navItem);
            }
        }

        private void UpdateProgress()
        {
            if (_mobileData.ProgressState == Enums.InstructionProgress.NotStarted)
            {
                _mobileData.ProgressState = Enums.InstructionProgress.Driving;
                _mobileApplicationDataChunkService.CurrentMobileData = _mobileData;
                _mobileApplicationDataChunkService.Commit();
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
            NavItem<Item> navItem = new NavItem<Item>() { ID = order.ID, ParentID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString(); }
        }

        #endregion



    }
}
