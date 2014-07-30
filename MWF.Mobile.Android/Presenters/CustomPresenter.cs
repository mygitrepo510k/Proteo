using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Presentation;
using Android.Content;

namespace MWF.Mobile.Android.Presenters
{

    public interface IFragmentHost
    {
        bool Show(MvxViewModelRequest request);
        bool Close(IMvxViewModel viewModel);
        MvxFragment CurrentFragment { get; }
        void CloseUpToView<TViewModel>() where TViewModel : IMvxViewModel;
        void CloseToInitialView();
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

        public override void Close(IMvxViewModel viewModel)
        {
            var currentFragmentHost = this.Activity as IFragmentHost;

            if (currentFragmentHost != null)
                if (currentFragmentHost.Close(viewModel))
                    return;

            base.Close(viewModel);
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            if (hint is Core.Presentation.CloseToInitialViewPresentationHint)
            {
                var currentFragmentHost = this.Activity as IFragmentHost;

                if (currentFragmentHost != null)
                {
                    currentFragmentHost.CloseToInitialView();
                    return;
                }
            }
            else
            {
                var hintType = hint.GetType();

                if (hintType.IsGenericType)
                {
                    if (hintType.GetGenericTypeDefinition() == typeof(Core.Presentation.CloseUpToViewPresentationHint<>))
                    {
                        var typeParameter = hintType.GetGenericArguments().First();

                        if (typeof(IMvxViewModel).IsAssignableFrom(typeParameter))
                        {
                            var currentFragmentHost = this.Activity as IFragmentHost;

                            if (currentFragmentHost != null)
                            {
                                var method = currentFragmentHost.GetType().GetMethod("CloseUpToView");
                                method.MakeGenericMethod(typeParameter).Invoke(currentFragmentHost, null);
                                return;
                            }
                        }
                    }
                }
            }

            base.ChangePresentation(hint);
        }

        public MvxViewModel CurrentActivityViewModel
        {
            get
            {
                return (this.Activity as MvxActivity).DataContext as MvxViewModel;
            }
        }

    }

}