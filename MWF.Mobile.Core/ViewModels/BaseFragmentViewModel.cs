using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.CrossCore;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Messages;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseFragmentViewModel
        : MvxViewModel, Portable.IVisible
    {

        abstract public string FragmentTitle { get; }
        private IMvxMessenger _messenger;
        private bool _isVisible;
        private MvxSubscriptionToken _modalSubscriptionToken = null;

        protected IMvxMessenger Messenger
        {
            get { return (_messenger = _messenger ?? Mvx.Resolve<IMvxMessenger>()); }
        }

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
            where TViewModel : IModalViewModel<TResult>
        {
            _modalSubscriptionToken = Messenger.SubscribeOnMainThread<ModalNavigationResultMessage<TResult>>(msg =>
            {
                // make sure message ids match up
                if (msg.MessageId == navItem.NavGUID)
                {
                    if (_modalSubscriptionToken != null)
                        Messenger.Unsubscribe<ModalNavigationResultMessage<TResult>>(_modalSubscriptionToken);

                    onResult(msg.Result);
                }
            });

            return ShowViewModel<TViewModel>(navItem);
        }

        #region IVisible

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        #endregion IVisible

    }

}