using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
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
        : BaseFragmentViewModel
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private IEnumerable<Models.Trailer> _trailers;
        private MvxCommand _selectTrailerCommand;


        #endregion

        #region Construction

        public InstructionTrailerViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;

            _repositories = repositories;
            Trailers = _repositories.TrailerRepository.GetAll();

        }

        public void Init(NavItem<MobileData> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
            isTrailerSelection = true;
        }

        public void Init(NavItem<Models.Instruction.Trailer> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
            isTrailerEdit = true;
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

        public bool isTrailerEdit { get; set; }

        public bool isTrailerSelection { get; set; }


        #endregion

        #region Private Methods

        private void SelectTrailer()
        {
            if(isTrailerEdit)
            {
                NavItem<Models.Instruction.Trailer> navItem = new NavItem<Models.Instruction.Trailer>() { ID = _mobileData.ID };
                _navigationService.MoveToNext(navItem);
            }
            else if(isTrailerSelection)
            {
                NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
                _navigationService.MoveToNext(navItem);
            }            
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Trailer"; }
        }

        #endregion

    }
}
