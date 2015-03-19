using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class MessageViewModel
        : BaseFragmentViewModel, IBackButtonHandler
    {

        #region Private Members

        private MobileData _mobileData;
        private NavData<MobileData> _navData;
        private IRepositories _repositories;
        private INavigationService _navigationService;

        private MvxCommand _readMessageCommand;

        #endregion Private Members

        #region Construction

        public MessageViewModel(INavigationService navigationService, IRepositories repositories, IMainService mainService)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;
        }

        #endregion Construction

        #region Public Properties

        public string MessageContentText { get { return _mobileData.Order.Items.First().Description; } }

        public string Address
        {
            get
            {
                return (this.isWithPoint)
                    ? _mobileData.Order.Addresses[0].Lines.Replace("|", "\n") + "\n" + _mobileData.Order.Addresses[0].Postcode
                    : string.Empty;
            }
        }

        public string PointDescription { get { return _mobileData.Order.Description; } }

        public string AddressLabelText { get { return "Address"; } }

        public string ReadButtonText { get { return "Mark as read"; } }

        public bool isWithPoint { get { return _mobileData.Order.Addresses.Count > 0; } }

        public ICommand ReadMessageCommand
        {
            get
            {
                return (_readMessageCommand = _readMessageCommand ?? new MvxCommand(() => ReadMessage()));
            }
        }

        #endregion Public Properties

        #region Private Methods

        private void ReadMessage()
        {
            _navigationService.MoveToNext(_navData);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
            _navData.Data = _mobileData;
            RaiseAllPropertiesChanged();
        }

        #endregion Private Methods

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle { get { return (isWithPoint) ? "Message with a Point" : "Message"; } }

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
    }
}
