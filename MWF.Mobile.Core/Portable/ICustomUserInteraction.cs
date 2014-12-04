﻿using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Portable
{
    public interface ICustomUserInteraction
    {
        void PopUpImage(byte[] bytes, string message, Action done = null, string title = "", string okButton = "OK");
        Task PopUpImageAsync(byte[] bytes, string message, string title = "", string okButton = "OK");

        void PopUpInstructionNotifaction(List<MobileData> alteredInstructions, Action done = null, string title = "", string okButton = "OK");
        void PopUpCurrentInstructionNotifaction(string message, Action done = null, string title = "", string okButton = "OK");
    }
}
