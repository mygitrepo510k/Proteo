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
        : BaseModalViewModel<bool>,
        IBackButtonHandler
    {

        #region Private Members

        private MobileData _mobileData;

        private IRepositories _repositories;

        private IDataChunkService _dataChunkService;
        private IMainService _mainService;

        private MvxCommand _readMessageCommand;

        private bool _isMessageRead;

        #endregion Private Members

        #region Construction

        public MessageViewModel(
            IRepositories repositories, 
            IMainService mainService, 
            IDataChunkService dataChunkService)
        {
            _dataChunkService = dataChunkService;
            _mainService = mainService;

            _repositories = repositories;
        }

        public void Init(MessageModalNavItem navData)
        {
            GetMobileDataFromRepository(navData.MobileDataID);
            _isMessageRead = navData.IsRead;
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

        public string ReadButtonText { get { return _isMessageRead ? "Return" : "Mark as read"; } }

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
            if (!_isMessageRead)
            {
                _mobileData.ProgressState = Enums.InstructionProgress.Complete;

                _dataChunkService.SendDataChunk(new MobileApplicationDataChunkContentActivity(), _mobileData, _mainService.CurrentDriver, _mainService.CurrentVehicle);
            }
            ReturnResult(!_isMessageRead);
        }

        private void GetMobileDataFromRepository(Guid ID)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(ID);
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

            ReturnResult(false);

            return task;
        }

        #endregion IBackButtonHandler Implementation

    }
}
