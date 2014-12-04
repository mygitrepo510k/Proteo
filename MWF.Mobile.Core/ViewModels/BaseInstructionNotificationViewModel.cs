using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    /// <summary>
    /// This base class is used to recieve notifications for when instructions that are being viewed are updated or deleted.
    /// </summary>
    public abstract class BaseInstructionNotificationViewModel
        : BaseFragmentViewModel
    {
        private MvxSubscriptionToken _notificationToken;

        public BaseInstructionNotificationViewModel()
        {
            _notificationToken = Messenger.Subscribe<Messages.GatewayInstructionNotificationMessage>(m =>
                CheckInstructionNotification(m.Command, m.InstructionID)
                );
        }

        private IMvxMessenger _messenger;



        protected new IMvxMessenger Messenger
        {
            get
            {
                return (_messenger = _messenger ?? Mvx.Resolve<IMvxMessenger>());
            }
        }

        abstract public void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID);
    }
}
