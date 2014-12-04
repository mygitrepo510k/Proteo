using Cirrious.MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Messages
{
    public class GatewayInstructionNotificationMessage
        :MvxMessage
    {
        public enum NotificationCommand { Add, Update, Delete };

        public GatewayInstructionNotificationMessage(object sender, NotificationCommand command, Guid instructionID)
            :base(sender)
        {
            Command = command;
            InstructionID = instructionID;
        }

        public NotificationCommand Command { get; private set; }
        public Guid InstructionID { get; private set; }
    }
}
