using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;
using System.Linq;
using System;
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
            _mobileDataRepository.GetInProgressInstructions();
            _navigationService = navigationService;

            var testOrder = new MobileData() { EffectiveDate = DateTime.Now.AddMonths(1), Title = "Proteo Test Client" };
            var inProgress = new Section()
            {
                Header = "In Progress",
                Instructions = _mobileDataRepository.GetInProgressInstructions()
            };
            var notStarted = new Section()
            {
                Header = "Not Started",
                Instructions = _mobileDataRepository.GetNotStartedInstructions()
            };
            Sections = new List<Section>() { inProgress, notStarted }.AsEnumerable<Section>();
        }

        public override string FragmentTitle
        {
            get { return "Manifest"; }
        }

        public string HeaderText
        {
            get { return "Select instructions - Showing " + InsructionsCount; }
        }


        private IEnumerable<Section> _sections;
        public IEnumerable<Section> Sections
        {
            get { return _sections; }
            set { _sections = value; RaisePropertyChanged(() => Sections); }
        }

        public string _dateOutput;
        public string DateOutput
        {
            get { return _dateOutput; }
            set { _dateOutput = value; RaisePropertyChanged(() => DateOutput); }
        }

        public int InsructionsCount
        {
            get { return Sections.Sum(s => s.Count()); }
        }

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
        
    }

    public class Section : IEnumerable<MobileData>
    {

        public string Header { get; set; }
        public IEnumerable<MobileData> Instructions { get; set; }

        public IEnumerator<MobileData> GetEnumerator()
        {
            return Instructions.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
