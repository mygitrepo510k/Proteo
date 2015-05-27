using System;


namespace MWF.Mobile.Core.ViewModels.Interfaces
{
    public interface IInstructionNotificationViewModel : MWF.Mobile.Core.Portable.IDisposable
    {
        System.Threading.Tasks.Task CheckInstructionNotificationAsync(MWF.Mobile.Core.Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID);
        void UnsubscribeNotificationToken();
    }
}
