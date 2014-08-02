using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;

        #endregion

        #region Construction

        public InstructionViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavItem<MobileData> item)
        {
            Guid guid = item.ID;
        }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Collect"; }
        }

        #endregion


    }
}
