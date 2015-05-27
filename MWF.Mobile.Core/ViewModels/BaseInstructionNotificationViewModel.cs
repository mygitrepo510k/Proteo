using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{

    /// <summary>
    /// This base class is used to recieve notifications for when instructions that are being viewed are updated or deleted.
    /// </summary>
    public abstract class BaseInstructionNotificationViewModel
        : BaseFragmentViewModel, MWF.Mobile.Core.Portable.IDisposable, IInstructionNotificationViewModel
    {

        private MvxSubscriptionToken _notificationToken;

        public BaseInstructionNotificationViewModel()
        {
            _notificationToken = Messenger.Subscribe<Messages.GatewayInstructionNotificationMessage>(async m => await CheckInstructionNotificationAsync(m.Command, m.InstructionID));
        }

        private IMvxMessenger _messenger;



        protected new IMvxMessenger Messenger
        {
            get
            {
                return (_messenger = _messenger ?? Mvx.Resolve<IMvxMessenger>());
            }
        }

        public void UnsubscribeNotificationToken()
        {
            if (_notificationToken != null)
                Messenger.Unsubscribe<Messages.GatewayInstructionNotificationMessage>(_notificationToken);
        }

        abstract public Task CheckInstructionNotificationAsync(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID);

        public void Dispose()
        {
            UnsubscribeNotificationToken();
        }

    }

}
