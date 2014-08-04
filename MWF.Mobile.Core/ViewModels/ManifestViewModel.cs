using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MWF.Mobile.Core.Models;
using System.Linq;
using System;
using System.Windows.Input;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.ViewModels.Interfaces;


namespace MWF.Mobile.Core.ViewModels
{

    public class ManifestViewModel 
		: BaseFragmentViewModel , IBackButtonHandler
    {

        private readonly IMobileDataRepository _mobileDataRepository;
        private readonly INavigationService _navigationService;

        public ManifestViewModel(IMobileDataRepository mobileDataRepository, INavigationService navigationService)
        {
            _mobileDataRepository = mobileDataRepository;
            var all = _mobileDataRepository.GetAll();
            var inProgressRepository = _mobileDataRepository.GetInProgressInstructions().ToList();
            var notStartedRepository = _mobileDataRepository.GetNotStartedInstructions().ToList();

            _navigationService = navigationService;
            Sections = new ObservableCollection<ManifestSectionViewModel>();
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


        private ObservableCollection<ManifestSectionViewModel> _sections;
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

        public string HeaderText
        {
            get { return "Select instructions - Showing " + InstructionsCount; }
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
