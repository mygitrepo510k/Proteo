using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Renderscripts;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Android.Views.Fragments;
using Type = System.Type;
using MWF.Mobile.Core.ViewModels.Interfaces;


namespace MWF.Mobile.Android.Views
{

    [Activity]
    public abstract class BaseActivityView
        : MvxActivity, Presenters.IFragmentHost
    {

        protected abstract Type GetFragmentTypeForViewModel(Type viewModelType);     

        protected BaseActivityViewModel BaseActivityViewModel
        {
            get { return (BaseActivityViewModel)this.ViewModel; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Populate FrameLayout with initial viewmodel fragment
            var initialViewModel = this.BaseActivityViewModel.InitialViewModel;
            var fragmentType = GetFragmentTypeForViewModel(initialViewModel.GetType());

            var fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
            fragment.ViewModel = initialViewModel;

            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_host, fragment);
            transaction.Commit();

        }

        public async override void OnBackPressed()
        {
            if (CurrentFragment.DataContext is IBackButtonHandler)
            {
                var continueBack = await (CurrentFragment.DataContext as IBackButtonHandler).OnBackButtonPressed();

                if (continueBack)
                {
                    base.OnBackPressed();
                    SetActivityTitleFromFragment();
                }
            }
            else
            {
                base.OnBackPressed();
                SetActivityTitleFromFragment();
            }
        }

        private void SetActivityTitleFromFragment()
        {
            this.ActionBar.Title = ((BaseFragmentViewModel)this.CurrentFragment.DataContext).FragmentTitle;
        }


        #region Fragment host

        private static IDictionary<Type, Type> _supportedFragmentViewModels = new Dictionary<Type, Type>
        {
            { typeof(Core.ViewModels.PasscodeViewModel), typeof(Fragments.PasscodeFragment) },
            { typeof(Core.ViewModels.CustomerCodeViewModel), typeof(Fragments.CustomerCodeFragment)},
            { typeof(Core.ViewModels.VehicleListViewModel), typeof(Fragments.VehicleListFragment)},
            { typeof(Core.ViewModels.TrailerListViewModel), typeof(Fragments.TrailerListFragment)},
            { typeof(Core.ViewModels.AboutViewModel), typeof(Fragments.AboutFragment)},
            { typeof(Core.ViewModels.OdometerViewModel), typeof(Fragments.OdometerFragment)},
            { typeof(Core.ViewModels.ManifestViewModel), typeof(Fragments.ManifestFragment)},
            { typeof(Core.ViewModels.SafetyCheckViewModel), typeof(Fragments.SafetyCheckFragment)},
			{ typeof(Core.ViewModels.SafetyCheckFaultViewModel), typeof(Fragments.SafetyCheckFaultFragment)},
            { typeof(Core.ViewModels.SafetyCheckSignatureViewModel), typeof(Fragments.SafetyCheckSignatureFragment) },
        };

        public bool Show(MvxViewModelRequest request)
        {
            // At this point simply display any supported fragment in the FrameLayout.
            // In future we may change this for different device form factors, for example to display tiled fragments.
            // For this reason I have duplicated this code in both MainView and StartupView, rather than abstracting
            // to a base class, since we may wish to handle fragements differently in each at some point.
            MvxFragment fragment = null;

            if (_supportedFragmentViewModels.ContainsKey(request.ViewModelType))
            {
                var fragmentType = _supportedFragmentViewModels[request.ViewModelType];
                fragment = (MvxFragment)Activator.CreateInstance(fragmentType);
                fragment.LoadViewModelFrom(request, null);
            }

            if (fragment == null)
                return false;

            var viewModel = fragment.ViewModel;
            this.ActionBar.Title = ((BaseFragmentViewModel)viewModel).FragmentTitle;

            var transaction = FragmentManager.BeginTransaction();
            // Note: this *replaces* the actual fragment host specified in Page_StartUp.axml
            // but keeps the same id
            transaction.Replace(Resource.Id.fragment_host, fragment);
            transaction.AddToBackStack(null);
            transaction.Commit();

            return true;
        }

        /// <summary>
        /// Given a view model attempts to close the fragment associated with it. If the fragment
        /// associated is currently being displayed in the fragment host then the backstack
        /// is popped.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public bool Close(IMvxViewModel viewModel)
        {
            var fragmentTypeToClose = _supportedFragmentViewModels[viewModel.GetType()];

            if (CurrentFragment != null && CurrentFragment.GetType() == fragmentTypeToClose)
            {
                FragmentManager.PopBackStackImmediate();

                SetActivityTitleFromFragment();

                return true;
            }
            else return false;
        }

        /// <summary>
        /// Remove all fragments from the back stack up to, but not including, the initial view.
        /// </summary>
        public void CloseToInitialView()
        {
            var backStackEntryCount = FragmentManager.BackStackEntryCount;

            for (var i = 0; i < backStackEntryCount; i++)
            {
                FragmentManager.PopBackStackImmediate();
            }

            SetActivityTitleFromFragment();
        }

        /// <summary>
        /// Remove all fragments from the back stack up to, but not including the specified view.
        /// </summary>
        /// <remarks>
        /// If the specified view is not found then all fragments will be removed up to, but not including, the initial view.
        /// </remarks>
        public void CloseUpToView<TViewModel>()
            where TViewModel : IMvxViewModel
        {
            var targetFragmentType = _supportedFragmentViewModels[typeof(TViewModel)];
            var backStackEntryCount = FragmentManager.BackStackEntryCount;

            for (var i = 0; i < backStackEntryCount; i++)
            {
                if (CurrentFragment.GetType() == targetFragmentType)
                    break;

                FragmentManager.PopBackStackImmediate();
            }

            this.SetActivityTitleFromFragment();
        }

        // Current Fragment in the fragment host. Note although the id being used appears to be that of the 
        // original container, it gets replaced during a show by the new fragment *but* keeps its old id.
        public MvxFragment CurrentFragment
        {
            get { return FragmentManager.FindFragmentById(Resource.Id.fragment_host) as MvxFragment; }
        }

        #endregion
    }

}