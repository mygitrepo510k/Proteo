using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionSignatureViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private INavigationService _navigationService;


        #endregion

        #region Construction

        public InstructionSignatureViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }


        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Sign for Collection"; }
        }

        #endregion
    }
}
