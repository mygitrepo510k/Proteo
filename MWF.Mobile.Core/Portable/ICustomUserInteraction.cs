using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Portable
{
    public interface ICustomUserInteraction
    {
        void PopUpImage(byte[] bytes, string message, Action done = null, string title = "", string okButton = "OK");
        Task PopUpImageAsync(byte[] bytes, string message, string title = "", string okButton = "OK");

        void PopUpInstructionNotification(List<ManifestInstructionViewModel> alteredInstructions, Action<List<ManifestInstructionViewModel>> done = null, string title = "", string okButton = "OK");
        void PopUpAlert(string message, Action done = null, string title = "", string okButton = "OK");

        void PopUpConfirm(string message, Action<bool> answer, string title = null, string okButton = "OK", string cancelButton = "Cancel");
    }
}
