using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Android.Presenters
{

    public interface IFragmentHost
    {
        bool Show(MvxViewModelRequest request);
    }

    public interface ICustomPresenter
    {
    }

    /// <summary>
    /// Custom presenter allowing fragments to be rendered within activities using MVVMCross's ShowViewModel.
    /// Activities that can contain fragments should implement IFragmentHost.
    /// </summary>
    public class CustomPresenter
        : MvxAndroidViewPresenter, ICustomPresenter
    {

        public override void Show(MvxViewModelRequest request)
        {
            var currentFragmentHost = this.Activity as IFragmentHost;

            if (currentFragmentHost != null)
                if (currentFragmentHost.Show(request))
                    return;

            base.Show(request);
        }

    }

}