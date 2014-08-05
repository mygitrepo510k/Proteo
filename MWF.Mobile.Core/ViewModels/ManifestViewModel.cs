using System.Linq;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.ObjectModel;
using System;
using System.Windows.Input;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.Models.Instruction;


namespace MWF.Mobile.Core.ViewModels
{

    public class ManifestViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        #region Private Members

        private readonly IMobileDataRepository _mobileDataRepository;
        private readonly INavigationService _navigationService;
        private readonly IReachability _reachability;
        private readonly IToast _toast;
        private readonly IGatewayPollingService _gatewayPollingService;
        private readonly IGatewayQueuedService _gatewayQueuedService;

        private ObservableCollection<ManifestSectionViewModel> _sections;
        private MvxCommand _refreshListCommand;
        private ManifestSectionViewModel _notStartedSection;
        private ManifestSectionViewModel _inProgressSection;

        #endregion

        #region Constructor

        public ManifestViewModel(IMobileDataRepository mobileDataRepository, INavigationService navigationService, IReachability reachability, IToast toast, IGatewayPollingService gatewayPollingService, IGatewayQueuedService gatewayQueuedService)
        {

            _mobileDataRepository = mobileDataRepository;

 
            _navigationService = navigationService;
            _reachability = reachability;
            _toast = toast;
            _gatewayPollingService = gatewayPollingService;
            _gatewayQueuedService = gatewayQueuedService;

            CreateSections();
            UpdateInstructions();
            
        }



        private void CreateSections()
        {
            Sections = new ObservableCollection<ManifestSectionViewModel>();

            _inProgressSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "In Progress",
            };

            Sections.Add(_inProgressSection);


            _notStartedSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Not Started",
            };


            Sections.Add(_notStartedSection);

            /**
            messageInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test 2 days", OrderID = "Test reg", EffectiveDate = DateTime.Now.AddDays(2), InstructionType = Enums.InstructionType.MessageWithPoint });

            //Sorts the list into ascending order.
            messageInstructions = new ObservableCollection<ManifestInstructionViewModel>(messageInstructions.ToList().OrderBy(x => x.EffectiveDate));
            var messageSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Messages",
                Instructions = messageInstructions
            };

            Sections.Add(messageSection);
             */ 


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
            get { return Sections.Sum(s => s.Instructions.Count); }
        }

        public ICommand RefreshListCommand
        {
            get
            {
                return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(async () => await UpdateInstructionsListAsync()));
            }
        }

        public string HeaderText
        {
            get { return "Select instructions - Showing " + InstructionsCount; }
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
                await _gatewayPollingService.PollForInstructions();

                UpdateInstructions();

                // Force a upload of the queue
                //await _gatewayQueuedService.UploadQueue();
            }
        }

        private void UpdateInstructions()
        {
            // get instruction data models from repository and order them
            var inProgressDataModels = _mobileDataRepository.GetInProgressInstructions().OrderBy(x => x.EffectiveDate);
            var notStartedDataModels = _mobileDataRepository.GetNotStartedInstructions().OrderBy(x => x.EffectiveDate);

            // Create the view models
            var inProgressViewModels = inProgressDataModels.Select(md => new ManifestInstructionViewModel(_navigationService, md));
            var notStartedViewModels = notStartedDataModels.Select(md => new ManifestInstructionViewModel(_navigationService, md));

            // Update the obsercable collections in each section
            _inProgressSection.Instructions = new ObservableCollection<ManifestInstructionViewModel>(inProgressViewModels);
            _notStartedSection.Instructions = new ObservableCollection<ManifestInstructionViewModel>(notStartedViewModels);

            // Let the UI know the number of instructions has changed
            RaisePropertyChanged(() => InstructionsCount);
            RaisePropertyChanged(() => HeaderText);
        }



        #endregion

        #region IBackButtonHandler Implementation

        public async Task<bool> OnBackButtonPressed()
        {

            bool continueWithBackPress = await Mvx.Resolve<IUserInteraction>().ConfirmAsync("Do you wish to logout?", "Changes will be lost!");

            if (continueWithBackPress)
            {
                if (_navigationService.IsBackActionDefined())
                {
                    //todo: move this to base class
                    _navigationService.GoBack();
                    return false;
                }

                return true;
            }

            return false;

        }

        #endregion
    }
}
