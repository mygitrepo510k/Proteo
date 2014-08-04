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
        private int _instructionCount;
        private MvxCommand _refreshListCommand;

        #endregion

        #region Constructor

        public ManifestViewModel(IMobileDataRepository mobileDataRepository, INavigationService navigationService, IReachability reachability, IToast toast, IGatewayPollingService gatewayPollingService, IGatewayQueuedService gatewayQueuedService)
        {
            _mobileDataRepository = mobileDataRepository;
            var all = _mobileDataRepository.GetAll();
            var inProgressRepository = _mobileDataRepository.GetInProgressInstructions().ToList();
            var notStartedRepository = _mobileDataRepository.GetNotStartedInstructions().ToList();

            _navigationService = navigationService;
            _reachability = reachability;
            _toast = toast;
            _gatewayPollingService = gatewayPollingService;
            _gatewayQueuedService = gatewayQueuedService;

            var Sections = new ObservableCollection<ManifestSectionViewModel>();
            ObservableCollection<ManifestInstructionViewModel> inProgressInstructions = new ObservableCollection<ManifestInstructionViewModel>();
            ObservableCollection<ManifestInstructionViewModel> notStartedInstructions = new ObservableCollection<ManifestInstructionViewModel>();

            //Test data
            inProgressInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test title", OrderID = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            inProgressInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test title2", OrderID = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });           


            foreach (var child in inProgressRepository)
            {
                inProgressInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionID = child.ID ,EffectiveDate = child.EffectiveDate, InstructionType = child.Order.Type, OrderID = child.Order.OrderId, InstructionTitle = child.GroupTitle });
            }

            inProgressInstructions = new ObservableCollection<ManifestInstructionViewModel>(inProgressInstructions.ToList().OrderBy(x => x.EffectiveDate));
            var inProgressSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "In Progress",
                Instructions = inProgressInstructions
            };

            Sections.Add(inProgressSection);

            //Test data
            notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test Collect", OrderID = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test Deliver", OrderID = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });
            notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test 1 day", OrderID = "Test reg", EffectiveDate = DateTime.Now.AddDays(1), InstructionType = Enums.InstructionType.TrunkTo });
            notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test Proceed From", OrderID = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.ProceedFrom });
            notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionTitle = "Test 2 days", OrderID = "Test reg", EffectiveDate = DateTime.Now.AddDays(2), InstructionType = Enums.InstructionType.MessageWithPoint });

            foreach (var child in notStartedRepository)
            {
                notStartedInstructions.Add(new ManifestInstructionViewModel(_navigationService) { InstructionID = child.ID, EffectiveDate = child.EffectiveDate, InstructionType = child.Order.Type, OrderID = child.Order.OrderId, InstructionTitle = child.GroupTitle });
            }

            //Sorts the list into ascending order.
            notStartedInstructions = new ObservableCollection<ManifestInstructionViewModel>(notStartedInstructions.ToList().OrderBy(x => x.EffectiveDate));
            var notStartedSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Not Started",
                Instructions = notStartedInstructions
            };

            Sections.Add(notStartedSection);
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
            get { return _instructionCount; }
            set { _instructionCount = value; RaisePropertyChanged(() => InstructionsCount); }
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

        public async Task UpdateInstructionsListAsync()
        {

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
            {
                // Force a poll for instructions             
                await _gatewayPollingService.PollForInstructions();

                // Force a upload of the queue
                //await _gatewayQueuedService.UploadQueue();
            }
        }

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
