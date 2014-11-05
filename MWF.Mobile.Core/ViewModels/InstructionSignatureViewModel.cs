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
        private readonly IMobileApplicationDataChunkService _mobileApplicationDataChunkService;
        private MobileData _mobileData;

        #endregion

        #region Construction

        public InstructionSignatureViewModel(INavigationService navigationService, IRepositories repositories, IUserInteraction userInteraction, IMobileApplicationDataChunkService mobileApplicationDataChunkService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _userInteraction = userInteraction;
            _mobileApplicationDataChunkService = mobileApplicationDataChunkService;

        }

        public void Init(NavItem<MobileData> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
        }


        #endregion

        #region Public Properties

        public string InstructionSignatureButtonLabel { get { return "Complete"; } }

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

            if ((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerSignatureRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && _mobileData.Order.Additional.CustomerSignatureRequiredForDelivery)
                    && string.IsNullOrWhiteSpace(CustomerSignatureEncodedImage))
            {
                _userInteraction.Alert("Signature is required");
                return;
            }

            if ((_mobileData.Order.Type == Enums.InstructionType.Collect && _mobileData.Order.Additional.CustomerNameRequiredForCollection)
                    || (_mobileData.Order.Type == Enums.InstructionType.Deliver && _mobileData.Order.Additional.CustomerNameRequiredForDelivery)
                    && string.IsNullOrWhiteSpace(CustomerName))
            {
                _userInteraction.Alert("The signers name is required");
                return;
            }

            _mobileApplicationDataChunkService.CurrentDataChunkActivity.Signature = new Models.Signature { Title = CustomerName, EncodedImage = CustomerSignatureEncodedImage };

            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);

        }


        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Sign for " + ((_mobileData.Order.Type == Enums.InstructionType.Collect) ? "Collection" : "Delivery"); }
        }

        #endregion
    }
}
