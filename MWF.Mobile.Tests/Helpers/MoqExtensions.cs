using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Language.Flow;
using Moq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Cirrious.CrossCore.IoC;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Portable;

namespace MWF.Mobile.Tests.Helpers
{
    public static class TextHelperExtensions
    {
        // Allows specification of different return values from a mock on subsequent calls of a method
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,  params object[] results) where T : class
        {
            var queue = new Queue(results);
            setup.Returns(() =>
            {
                var result = queue.Dequeue();
                if (result is Exception)
                {
                    throw result as Exception;
                }
                return (TResult)result;
            });
        }

        public static Mock<T> InjectNewMock<T>(this IFixture fixture) where T : class
        {
            Mock<T> newMock = new Mock<T>();
            fixture.Inject<T>(newMock.Object);
            return newMock;
        }

        public static Mock<T> RegisterNewMock<T>(this IMvxIoCProvider ioc) where T : class
        {
            Mock<T> newMock = new Mock<T>();
            ioc.RegisterSingleton<T>(newMock.Object);
            return newMock;
        }

        public static MobileData SetUpInstruction(this IFixture fixture, MWF.Mobile.Core.Enums.InstructionType instructionType,
            bool isBypassCommentScreen, bool isTrailerConfirmationRequired, bool isCustomerNameRequired, bool isCustomerSignatureRequired, bool isScanRequiredForCollection, bool isScanRequiredForDelivery, MWF.Mobile.Core.Enums.InstructionProgress? instructionProgress)
        {
            var mobileData = fixture.Create<MobileData>();
            mobileData.Order.Type = instructionType;
            mobileData.Order.Additional.IsTrailerConfirmationEnabled = isTrailerConfirmationRequired;
            mobileData.Order.Items.First().Additional.BypassCommentsScreen = isBypassCommentScreen;

            if (instructionType == Core.Enums.InstructionType.Collect)
            {
                mobileData.Order.Additional.CustomerNameRequiredForCollection = isCustomerNameRequired;
                mobileData.Order.Additional.CustomerSignatureRequiredForCollection = isCustomerSignatureRequired;
                mobileData.Order.Items.ForEach(i => i.Additional.BarcodeScanRequiredForCollection = isScanRequiredForCollection);
            }
            else
            {
                mobileData.Order.Additional.CustomerNameRequiredForDelivery = isCustomerNameRequired;
                mobileData.Order.Additional.CustomerSignatureRequiredForDelivery = isCustomerSignatureRequired;
                mobileData.Order.Items.ForEach(i => i.Additional.BarcodeScanRequiredForDelivery = isScanRequiredForDelivery);
            }

            if (instructionProgress != null)
                mobileData.ProgressState = (MWF.Mobile.Core.Enums.InstructionProgress)instructionProgress;

            return mobileData;
        }

        #region IUserInteraction Helpers

        // shortcut way of setting up a Mock UserInteraction to execute the "OK" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsTrueIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));

            return userInteractionMock;
        }

        // shortcut way of setting up a Mock UserInteraction to execute the "OK" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmAsyncReturnsTrueIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.ConfirmAsync(It.IsAny<String>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Returns(Task.FromResult(true));

            return userInteractionMock;
        }

        // shortcut way of setting up a Mock UserInteraction to execute the "Cancel" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmAsyncReturnsFalseIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.ConfirmAsync(It.IsAny<String>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Returns(Task.FromResult(false));

            return userInteractionMock;
        }


        // shortcut way of setting up a Mock UserInteraction to execute the "Cancel" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsFalseIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(false));

            return userInteractionMock;
        }

        // shortcut way of setting up a Mock CustomUserInteraction to execute the "OK" logic of a Confirm call 
        public static Mock<ICustomUserInteraction> ConfirmReturnsTrueIfTitleStartsWith(this Mock<ICustomUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.PopUpConfirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));

            return userInteractionMock;
        }


        // shortcut way of setting up a Mock CustomUserInteraction to execute the "Cancel" logic of a Confirm call 
        public static Mock<ICustomUserInteraction> ConfirmReturnsFalseIfTitleStartsWith(this Mock<ICustomUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.PopUpConfirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(false));

            return userInteractionMock;
        }

        // shortcut way of setting up a Mock UserInteraction to execute the "OK" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsTrue(this Mock<IUserInteraction> userInteractionMock)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));

            return userInteractionMock;
        }


        // shortcut way of setting up a Mock UserInteraction to execute the "Cancel" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsFalse(this Mock<IUserInteraction> userInteractionMock)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(false));

            return userInteractionMock;
        }

        public static Mock<IUserInteraction> AlertInvokeAction(this Mock<IUserInteraction> userInteractionMock)
        {
            userInteractionMock.Setup(ui => ui.Alert(It.IsAny<String>(), It.Is<Action>(a => a != null), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action, string, string>((s1, a, s2, s3) => a.Invoke());

            return userInteractionMock;
        }

        #endregion

    }
}
