using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Messages;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseActivityViewModel 
		: MvxViewModel
    {

        public IMvxViewModel InitialViewModel { get; protected set; }
        
        protected void CloseToInitialView()
        {
            this.ChangePresentation(new Presentation.CloseToInitialViewPresentationHint());
        }

        protected void CloseUpToView<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            this.ChangePresentation(new Presentation.CloseUpToViewPresentationHint<TViewModel>());
        }

    }

}
