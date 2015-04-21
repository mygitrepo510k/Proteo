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
        private readonly BaseFragmentViewModel _baseViewModel;

        public ManifestInstructionViewModel(BaseFragmentViewModel viewModel, INavigationService navigationService, MobileData mobileData)
        {
            _baseViewModel = viewModel;
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
            get { return (InstructionType == Enums.InstructionType.OrderMessage) ? GenerateMessageTypeText() : _mobileData.Order.RouteTitle; }
        }

        public string PointDescripion
        {
            get { return (InstructionType == Enums.InstructionType.OrderMessage) ? _mobileData.MessageText : _mobileData.Order.Description; }
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

        public Enums.InstructionProgress ProgressState
        {
            get
            {
                return _mobileData.ProgressState;
            }
        }


        private MvxCommand _selectInstructionCommand;
        public ICommand SelectInstructionCommand
        {
            get
            {
                if (InstructionType == default(Enums.InstructionType))
                    return _selectInstructionCommand;
                else if (InstructionType == Enums.InstructionType.OrderMessage)
                    return (_selectInstructionCommand = _selectInstructionCommand ?? new MvxCommand(OpenMessageModal));

                return (_selectInstructionCommand = _selectInstructionCommand ?? new MvxCommand(SelectInstruction));
            }
        }

        private void SelectInstruction()
        {
            var navItem = new NavData<MobileData>() { Data = _mobileData };
            _navigationService.MoveToNext(navItem);
        }

        private void OpenMessageModal()
        {
            var navItem = new MessageModalNavItem { MobileDataID = _mobileData.ID, IsRead = (_mobileData.ProgressState == Enums.InstructionProgress.Complete) };
            _baseViewModel.ShowModalViewModel<MessageViewModel, bool>(navItem, (sendChunk) =>
            {
                //This is to update any read messages in the inbox.
                //For some reason the Manifest screen doesn't need it because it just removes the items from the manifest.
                var inboxVM = _baseViewModel as InboxViewModel;

                if (inboxVM != null)
                    inboxVM.RefreshMessagesCommand.Execute(null);
            });
        }

        private string GenerateMessageTypeText()
        {
            if (_mobileData.Order.Addresses.Count > 0)
                return "Message with a Point";
            else
                return "Message";
        }
    }
}
