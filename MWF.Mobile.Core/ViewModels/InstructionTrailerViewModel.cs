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


        #endregion

        #region Construction

        public InstructionTrailerViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavItem<MobileData> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
        }


        #endregion

        #region Public Properties

        private MvxCommand _advanceInstructionTrailerCommand;
        public ICommand AdvanceInstructionTrailerCommand
        {
            get
            {
                return (_advanceInstructionTrailerCommand = _advanceInstructionTrailerCommand ?? new MvxCommand(() => AdvanceInstructionTrailer()));
            }
        }

        public string InstructionTrailerButtonLabel { get { return "Move on"; } }


        #endregion

        #region Private Methods

        private void AdvanceInstructionTrailer()
        {
            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
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
