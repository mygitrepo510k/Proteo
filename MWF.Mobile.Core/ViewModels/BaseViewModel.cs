using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Messages;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseViewModel 
		: MvxViewModel
    {

        public BaseViewModel()
        {

            Messenger = Mvx.Resolve<IMvxMessenger>();
        }

        protected IMvxMessenger Messenger { get; private set; }


        /// <summary>
        /// Shows a view modal that will return a result value when it is closed
        /// See http://www.gregshackles.com/2012/11/returning-results-from-view-models-in-mvvmcross/
        /// </summary> 
        /// <typeparam name="TViewModel">View Model to show (must be a subclass of  ModalBaseActivityModel) </typeparam>
        /// <typeparam name="TResult">The result type the modal view model will return</typeparam>
        /// <param name="navItem">Navigation object containing parameters for the modal view model</param>
        /// <param name="onResult"> Action to run when the modal view has closed, returning with a result</param>
        /// <returns></returns>
        public bool ShowModalViewModel<TViewModel, TResult>(IModalNavItem navItem, Action<TResult> onResult)
        where TViewModel : BaseModalViewModel<TResult>
        {

            navItem.MessageID = Guid.NewGuid();

            MvxSubscriptionToken token = null;
            token = Messenger.Subscribe<ModalNavigationResultMessage<TResult>>(msg =>
            {
                //make sure message ids match up
                if (token != null && msg.MessageId == navItem.MessageID)
                    Messenger.Unsubscribe<ModalNavigationResultMessage<TResult>>(token);

                onResult(msg.Result);
            });

            return ShowViewModel<TViewModel>(navItem);
        }

    }

}
