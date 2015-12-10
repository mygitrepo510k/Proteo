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

        private IMvxViewModel _initialViewModel;

        public IMvxViewModel InitialViewModel
        {
            get { return _initialViewModel; }
            protected set { _initialViewModel = value; RaisePropertyChanged(() => InitialViewModel); }
        }


        protected void CloseToInitialView()
        {
            this.ChangePresentation(new Presentation.CloseToInitialViewPresentationHint());
        }

        protected void CloseUpToView<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            this.ChangePresentation(new Presentation.CloseUpToViewPresentationHint(typeof(TViewModel)));
        }

        public bool Close()
        {
            return base.Close(this);
        }

    }

}
