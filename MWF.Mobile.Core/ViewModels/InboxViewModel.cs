﻿using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class InboxViewModel
        : BaseInstructionNotificationViewModel,
        IVisible
    {

        private IRepositories _repositories;
        private IMainService _mainService;
        private INavigationService _navigationService;
        private IGatewayPollingService _gatewayPollingService;
        private MvxCommand _refreshMessagesCommand;


        public InboxViewModel(
            IRepositories repositories, 
            IMainService mainService, 
            INavigationService navigationService,
            IGatewayPollingService gatewayPollingService)
        {
            _repositories = repositories;
            _mainService = mainService;
            _navigationService = navigationService;
            _gatewayPollingService = gatewayPollingService;

            RefreshMessages();
        }


        private ObservableCollection<ManifestInstructionViewModel> _messages;
        public ObservableCollection<ManifestInstructionViewModel> Messages
        {
            get { return _messages; }
            set { _messages = value; RaisePropertyChanged(() => Messages); }
        }

        public string InboxHeaderText
        {
            get { return "Showing " + MessagesCount + " messages"; }
        }

        public int MessagesCount
        {
            get { return Messages.ToList().Count; }
        }

        public ICommand RefreshMessagesCommand
        {
            get
            {
                return (_refreshMessagesCommand = _refreshMessagesCommand ?? new MvxCommand(() => RefreshMessages()));
            }
        }

        private void RefreshMessages()
        {
            _gatewayPollingService.PollForInstructions();
            ReloadPage();
        }

        private void ReloadPage()
        {
            Messages = new ObservableCollection<ManifestInstructionViewModel>(_repositories.MobileDataRepository.GetAllMessages(_mainService.CurrentDriver.ID).Select(m => new ManifestInstructionViewModel(this, _navigationService, m)).OrderBy(m => m.ProgressState).ThenBy(m => m.ArrivalDate));
            RaisePropertyChanged(() => MessagesCount);
            RaisePropertyChanged(() => InboxHeaderText);
        }


        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Inbox"; }
        }

        #endregion

        #region BaseInstructionNotificationViewModel Overrides

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            ReloadPage();
        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region IVisible

        public void IsVisible(bool isVisible)
        {
            if (isVisible) { }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        #endregion IVisible
    }
}
