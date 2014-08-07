using Chance.MvvmCross.Plugins.UserInteraction;
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
    public class InstructionSignatureViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IUserInteraction _userInteraction;
        private MobileData _mobileData;

        #endregion

        #region Construction

        public InstructionSignatureViewModel(INavigationService navigationService, IRepositories repositories, IUserInteraction userInteraction)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _userInteraction = userInteraction;

        }

        public void Init(NavItem<MobileData> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
        }


        #endregion

        #region Public Properties

        public string InstructionSignatureButtonLabel { get { return "Move on"; } }

        private string _customerName;
        public string CustomerName
        {
            get { return _customerName; }
            set { _customerName = value; RaisePropertyChanged(() => CustomerName); }
        }

        private string _customerSignatureEncodedImage;
        public string CustomerSignatureEncodedImage
        {
            get { return _customerSignatureEncodedImage; }
            set { _customerSignatureEncodedImage = value; RaisePropertyChanged(() => CustomerSignatureEncodedImage); }
        }

        private MvxCommand _instructionDoneCommand;
        public ICommand InstructionDoneCommand
        {
            get
            {
                return (_instructionDoneCommand = _instructionDoneCommand ?? new MvxCommand(() => InstructionDone()));
            }
        }

        #endregion

        #region Private Methods

        private void InstructionDone()
        {
            _mobileData.Order.Additional.CustomerSignatureRequiredForCollection = true;

            if(_mobileData.Order.Additional.CustomerSignatureRequiredForCollection && string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage))
            {
                    _userInteraction.Alert("Signature is required");
                    return;   
            }

            if (_mobileData.Order.Additional.CustomerNameRequiredForCollection && string.IsNullOrWhiteSpace(CustomerName))
            {
                _userInteraction.Alert("Your name is required");
                return;
            }

            /*
            // Set the signature on the vehicle and trailer safety checks
            foreach (var safetyCheckData in _safetyCheckData)
            {
                safetyCheckData.Signature = new Models.Signature { EncodedImage = this.SignatureEncodedImage };
            }

            // Complete the startup process
            _startupService.Commit();
             */
            _navigationService.MoveToNext();
           
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
