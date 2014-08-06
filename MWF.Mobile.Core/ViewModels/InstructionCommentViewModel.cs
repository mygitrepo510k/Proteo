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
    public class InstructionCommentViewModel
        : BaseFragmentViewModel
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;


        #endregion

        #region Construction

        public InstructionCommentViewModel(INavigationService navigationService, IRepositories repositories)
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

        private MvxCommand _advanceInstructionCommentCommand;
        public ICommand AdvanceInstructionCommentCommand
        {
            get
            {
                return (_advanceInstructionCommentCommand = _advanceInstructionCommentCommand ?? new MvxCommand(() => AdvanceInstructionComment()));
            }
        }

        public string InstructionCommentButtonLabel { get { return "Move on"; } }


        #endregion

        #region Private Methods

        private void AdvanceInstructionComment()
        {
            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Comment"; }
        }

        #endregion

    }
}
