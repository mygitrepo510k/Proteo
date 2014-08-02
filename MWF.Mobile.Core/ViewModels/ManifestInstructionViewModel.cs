using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{
    public class ManifestInstructionViewModel : MvxViewModel
    {

        private readonly INavigationService _navigationService;

        public ManifestInstructionViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private ManifestSectionViewModel _manifestSectionViewModel;

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(() => Title);}
        }

        private string _vehicleRegistration;
        public string VehicleRegistration
        {
            get { return _vehicleRegistration; }
            set { _vehicleRegistration = value; RaisePropertyChanged(() => VehicleRegistration); }
        }

        private DateTime _effectiveDate;
        public DateTime EffectiveDate
        {
            get { return _effectiveDate; }
            set { _effectiveDate = value; RaisePropertyChanged(() => EffectiveDate); }
        }
        


        private Enums.InstructionType _instructionType;
        public Enums.InstructionType InstructionType
        {
            get
            {
                return _instructionType;
            }
            set
            {
                _instructionType = value;
                RaisePropertyChanged(() => InstructionType);
            }
        }


        private MvxCommand _selectInstructionCommand;
        public ICommand SelectInstructionCommand
        {
            get
            {
                      
                return (_selectInstructionCommand = _selectInstructionCommand ?? new MvxCommand(SelectInstruction));
            }
        }

        private void SelectInstruction()
        {
            // Todo: guid passed here should be the guid of the mobile instruction data model this 
            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = Guid.NewGuid() };
            _navigationService.MoveToNext(navItem);
        }
    }
}
