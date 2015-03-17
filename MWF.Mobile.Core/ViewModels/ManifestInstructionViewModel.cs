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
        private readonly MobileData _mobileData;

        public ManifestInstructionViewModel(INavigationService navigationService, MobileData mobileData)
        {
            _navigationService = navigationService;
            _mobileData = mobileData;
        }


        public MobileData MobileData
        {
            get { return _mobileData; }
        }

        public Guid InstructionID
        {
            get { return _mobileData.ID; }
        }

        public string RunID
        {
            get { return _mobileData.Order.RouteTitle; }        
        }

        public string PointDescripion
        {
            get { return _mobileData.Order.Description; }
        }

        public DateTime ArrivalDate
        {
            get { return _mobileData.Order.Arrive; }
        }

        public Enums.InstructionType InstructionType
        {
            get
            {
                return _mobileData.Order.Type;
            }
        }


        private MvxCommand _selectInstructionCommand;
        public ICommand SelectInstructionCommand
        {
            get
            {
                if (InstructionType == default(Enums.InstructionType))
                    return _selectInstructionCommand;
                return (_selectInstructionCommand = _selectInstructionCommand ?? new MvxCommand(SelectInstruction));
            }
        }

        private void SelectInstruction()
        {
            NavData<MobileData> navItem = new NavData<MobileData>() { Data = _mobileData };
            _navigationService.MoveToNext(navItem);
        }
    }
}
