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

            _navigationService = navigationService;
            _reachability = reachability;
            _toast = toast;
            _gatewayPollingService = gatewayPollingService;
            _gatewayQueuedService = gatewayQueuedService;

            var instructions = new ObservableCollection<ManifestInstructionViewModel>();
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test title", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test title2", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });

            /*
             * TODO: Implement repository once they can get data from the database.
             * 
            _mobileDataRepository = mobileDataRepository;
            var all = _mobileDataRepository.GetAll();
            var inProgressRepository = _mobileDataRepository.GetInProgressInstructions();
            var sectionRepository = _mobileDataRepository.GetNotStartedInstructions();
            foreach (var child in inProgressRepository.ToList())
            {
                
            }
             */

            Sections = new ObservableCollection<ManifestSectionViewModel>();
            var inProgressSection = new ManifestSectionViewModel(this)
            {
                //Instructions = _mobileApplicationDataRepository.GetInProgressInstructions()
                SectionHeader = "In Progress",
                Instructions = instructions
            };
            _instructionCount = _instructionCount + inProgressSection.Instructions.Count;
            Sections.Add(inProgressSection);

            instructions = new ObservableCollection<ManifestInstructionViewModel>();
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test Collect", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test Deliver", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test Trunk To", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.TrunkTo });
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test Proceed From", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.ProceedFrom });
            instructions.Add(new ManifestInstructionViewModel(_navigationService) { Title = "Test Message with point", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.MessageWithPoint });

            var notStartedSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Not Started",
                Instructions = instructions

                //Instructions = _mobileApplicationDataRepository.GetNotStartedInstructions()
            };
            _instructionCount = _instructionCount + notStartedSection.Instructions.Count;
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
