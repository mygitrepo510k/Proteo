using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class ManifestInstructionViewModel : MvxViewModel
    {

        public ManifestInstructionViewModel()
        {          
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
    }
}
