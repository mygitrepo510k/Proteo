using System;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels.Interfaces
{
    public interface IInstructionNotificationViewModel : MWF.Mobile.Core.Portable.IDisposable
    {
        Task CheckInstructionNotificationAsync(MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID);
        void UnsubscribeNotificationToken();
    }
}
