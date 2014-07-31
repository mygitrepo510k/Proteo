using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MWF.Mobile.Core.Models;
using System.Linq;
using System;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.ViewModels.Interfaces;


namespace MWF.Mobile.Core.ViewModels
{

    public class ManifestViewModel 
		: BaseFragmentViewModel , IBackButtonHandler
    {

        private readonly IMobileDataRepository _mobileDataRepository;

        public ManifestViewModel(IMobileDataRepository mobileDataRepository)
        {

            //_mobileDataRepository = mobileDataRepository;
           // _mobileDataRepository.GetInProgressInstructions();
           


            ObservableCollection<ManifestInstructionViewModel> instructions = new ObservableCollection<ManifestInstructionViewModel>();
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test title", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test title2", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });

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
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test Collect", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Collect });
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test Deliver", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.Deliver });
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test Trunk To", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.TrunkTo });
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test Proceed From", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.ProceedFrom });
            instructions.Add(new ManifestInstructionViewModel() { Title = "Test Message with point", VehicleRegistration = "Test reg", EffectiveDate = DateTime.Now, InstructionType = Enums.InstructionType.MessageWithPoint });
            
            var notStartedSection = new ManifestSectionViewModel(this)
            {
                SectionHeader = "Not Started",
                Instructions = instructions

                //Instructions = _mobileApplicationDataRepository.GetNotStartedInstructions()
            };
            _instructionCount = _instructionCount + notStartedSection.Instructions.Count;
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

        private int _instructionCount;
        public int InstructionsCount
        {
            get { return _instructionCount; }
            set { _instructionCount = value; RaisePropertyChanged(() => InstructionsCount); }
        }

        public string HeaderText
        {
            get { return "Select instructions - Showing " + InstructionsCount; }
        } 
  

        public async Task<bool> OnBackButtonPressed()
        {

            bool continueWithBackPress = await Mvx.Resolve<IUserInteraction>().ConfirmAsync("Do you wish to logout?", "Changes will be lost!");

            if (continueWithBackPress)
            {
                ShowViewModel<PasscodeViewModel>();
            }

            return false;

        }   
    }
}
