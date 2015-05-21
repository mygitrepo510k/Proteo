using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;


namespace MWF.Mobile.Core.ViewModels
{

    public class ManifestViewModel : 
        BaseInstructionNotificationViewModel, 
        IBackButtonHandler
    {

        public class DummyMobileData : MobileData { }

        #region Private Members

        private readonly IMobileDataRepository _mobileDataRepository;
        private readonly IApplicationProfileRepository _applicationProfileRepository;

        private readonly INavigationService _navigationService;
        private readonly IReachability _reachability;
        private readonly IToast _toast;
        private readonly IGatewayPollingService _gatewayPollingService;
        private readonly IGatewayQueuedService _gatewayQueuedService;
        private readonly IStartupService _startupService;

        private IMainService _mainService;
        private ObservableCollection<ManifestSectionViewModel> _sections;
        private MvxCommand _refreshListCommand;
        private MvxCommand _refreshStatusesCommand;
        private ManifestSectionViewModel _nonActiveInstructionsSection;
        private ManifestSectionViewModel _activeInstructionsSection;
        private ManifestSectionViewModel _messageSection;
        private ApplicationProfile _appProfile;
        private bool _initialised;

        #endregion

        #region Constructor

        public ManifestViewModel(IRepositories repositories, INavigationService navigationService, IReachability reachability, IToast toast,
                                 IGatewayPollingService gatewayPollingService, IGatewayQueuedService gatewayQueuedService, IStartupService startupService, IMainService mainService)
        {

            _mobileDataRepository = repositories.MobileDataRepository;
            _applicationProfileRepository = repositories.ApplicationRepository;

            _navigationService = navigationService;
            _reachability = reachability;
            _toast = toast;
            _gatewayPollingService = gatewayPollingService;
            _gatewayQueuedService = gatewayQueuedService;
            _startupService = startupService;
            _mainService = mainService;

            _appProfile = _applicationProfileRepository.GetAll().First();

            _initialised = true;

            CreateSections();
            RefreshInstructions();

        }

        private void CreateSections()
        {
            Sections = new ObservableCollection<ManifestSectionViewModel>();

            _activeInstructionsSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Active Instruction",
            };

            Sections.Add(_activeInstructionsSection);

            _nonActiveInstructionsSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Instructions",
            };

            Sections.Add(_nonActiveInstructionsSection);

            _messageSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Messages",
            };

            Sections.Add(_messageSection);

        }

        #endregion

        #region Public Properties

        public ObservableCollection<ManifestSectionViewModel> Sections
        {
            get { return _sections; }
            set { _sections = value; RaisePropertyChanged(() => Sections); }
        }

        public override string FragmentTitle
        {
            get { return "Manifest"; }
        }

        public int InstructionsCount
        {
            get { return Sections.Sum(s => s.Instructions.Where(i => !(i.MobileData is DummyMobileData)).Count()); }
        }

        public ICommand RefreshListCommand
        {
            get { return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(async () => await UpdateInstructionsListAsync())); }
        }

        public ICommand RefreshStatusesCommand
        {
            get { return (_refreshStatusesCommand = _refreshStatusesCommand ?? new MvxCommand(() => RefreshInstructions())); }
        }

        #endregion

        #region Private Methods

        private async Task UpdateInstructionsListAsync()
        {
            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
            {
                // Force a poll for instructions             
                await _gatewayPollingService.PollForInstructionsAsync();
            }
        }

        private void RefreshInstructions()
        {
            Mvx.Trace("started refreshing manifest screen");

            if (!_initialised)
                return;

            var today = DateTime.Now;

            // get instruction data models from repository and order them
            var activeInstructionsDataModels = _mobileDataRepository.GetInProgressInstructions(_startupService.LoggedInDriver.ID);
            var nonActiveInstructionsDataModels = _mobileDataRepository.GetNotStartedInstructions(_startupService.LoggedInDriver.ID);

            activeInstructionsDataModels = activeInstructionsDataModels.Where(i => i.EffectiveDate < today.AddDays(_appProfile.DisplaySpan) && i.EffectiveDate > today.AddDays(-_appProfile.DisplayRetention)).OrderBy(x => x.EffectiveDate);
            nonActiveInstructionsDataModels = nonActiveInstructionsDataModels.Where(i => i.EffectiveDate < today.AddDays(_appProfile.DisplaySpan) && i.EffectiveDate > today.AddDays(-_appProfile.DisplayRetention)).OrderBy(x => x.EffectiveDate);

            var messageDataModels = _mobileDataRepository.GetNonCompletedMessages(_startupService.LoggedInDriver.ID).OrderBy(x => x.EffectiveDate);

            if (activeInstructionsDataModels.ToList().Count == 0)
            {
                List<DummyMobileData> noneShowingList = new List<DummyMobileData>();
                noneShowingList.Add(new DummyMobileData() { Order = new Order() { Description = "No Active Instructions" } });
                IEnumerable<MobileData> noneShowingEnumerable = noneShowingList;
                activeInstructionsDataModels = (IOrderedEnumerable<MobileData>)noneShowingEnumerable.OrderBy(x => 1);
            }

            if (nonActiveInstructionsDataModels.ToList().Count == 0)
            {
                List<MobileData> noneShowingList = new List<MobileData>();
                noneShowingList.Add(new DummyMobileData() { Order = new Order() { Description = "No Instructions" } });
                IEnumerable<MobileData> noneShowingEnumerable = noneShowingList;
                nonActiveInstructionsDataModels = (IOrderedEnumerable<MobileData>)noneShowingEnumerable.OrderBy(x => 1);
            }

            if (messageDataModels.ToList().Count == 0)
            {
                List<MobileData> noneShowingList = new List<MobileData>();
                noneShowingList.Add(new DummyMobileData() { Order = new Order() { Description = "No Messages" } });
                IEnumerable<MobileData> noneShowingEnumerable = noneShowingList;
                messageDataModels = (IOrderedEnumerable<MobileData>)noneShowingEnumerable.OrderBy(x => 1);
            }

            // Create the view models
            var activeInstructionsViewModels = activeInstructionsDataModels.Select(md => new ManifestInstructionViewModel(this,_navigationService, md));
            var nonActiveInstructionsViewModels = nonActiveInstructionsDataModels.Select(md => new ManifestInstructionViewModel(this,_navigationService, md));
            var messageViewModels = messageDataModels.Select(md => new ManifestInstructionViewModel(this,_navigationService, md));

            // Update the observable collections in each section
            _activeInstructionsSection.Instructions = new ObservableCollection<ManifestInstructionViewModel>(activeInstructionsViewModels.OrderBy(ivm => ivm.ArrivalDate));
            _nonActiveInstructionsSection.Instructions = new ObservableCollection<ManifestInstructionViewModel>(nonActiveInstructionsViewModels.OrderBy(ivm => ivm.ArrivalDate));
            _messageSection.Instructions = new ObservableCollection<ManifestInstructionViewModel>(messageViewModels.OrderBy(ivm => ivm.ArrivalDate));

            // Let the UI know the number of instructions has changed
            RaisePropertyChanged(() => InstructionsCount);
            RaisePropertyChanged(() => Sections);

            Mvx.Trace("finished refreshing manifest screen");

        }

        #endregion

        #region IBackButtonHandler Implementation

        public async Task<bool> OnBackButtonPressed()
        {
            bool continueWithBackPress = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("Do you wish to logout?", "", "Logout");

            if (continueWithBackPress)
            {
                if (_navigationService.IsBackActionDefined())
                {
                    //TODO: Update Safety Checks profile

                    
                    _navigationService.GoBack();
                    return false;
                }

                return true;
            }

            return false;

        }

        #endregion

        #region BaseInstructionNotificationViewModel

        public override Task CheckInstructionNotificationAsync(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {
            RefreshInstructions();
            return Task.FromResult(0);
        }

        #endregion BaseInstructionNotificationViewModel
    }

}
