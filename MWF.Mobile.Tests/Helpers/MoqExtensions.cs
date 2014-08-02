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


        #region IUserInteraction Helpers

        // shortcut way of setting up a Mock UserInteraction to execute the "OK" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsTrueIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
                    .Callback<string, Action<bool>, string, string, string>((s1, a, s2, s3, s4) => a.Invoke(true));

            return userInteractionMock;
        }


        // shortcut way of setting up a Mock UserInteraction to execute the "Cancel" logic of a Confirm call 
        public static Mock<IUserInteraction> ConfirmReturnsFalseIfTitleStartsWith(this Mock<IUserInteraction> userInteractionMock, string messageStartsWith)
        {
            userInteractionMock.Setup(ui => ui.Confirm(It.IsAny<String>(), It.IsAny<Action<bool>>(), It.Is<String>(s => s.StartsWith(messageStartsWith)), It.IsAny<String>(), It.IsAny<String>()))
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

        #endregion

    }
}
