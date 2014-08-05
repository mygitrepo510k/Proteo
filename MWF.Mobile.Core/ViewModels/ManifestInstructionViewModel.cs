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


        public Guid InstructionID
        {
            get { return _mobileData.ID; }
        }

        public string InstructionTitle
        {
            get { return _mobileData.GroupTitle; }        
        }

        public string OrderID
        {
            get { return _mobileData.Order.OrderId; }
        }

        public DateTime EffectiveDate
        {
            get { return _mobileData.EffectiveDate; }
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
                      
                return (_selectInstructionCommand = _selectInstructionCommand ?? new MvxCommand(SelectInstruction));
            }
        }

        private void SelectInstruction()
        {
            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }
    }
}
