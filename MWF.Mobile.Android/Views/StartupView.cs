using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile;
using Android.Support.V4.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;
using support = Android.Support.V4.App;
using MWF.Mobile.Android.Helpers;

namespace MWF.Mobile.Android.Views
{

    [Activity(ScreenOrientation= ScreenOrientation.Portrait)]
    public class StartupView
        : BaseActivityView, Presenters.IFragmentHost
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Page_Startup);

            // Create the gateway queue timer service which will be available as a background service from here, regardless of activity.
            // Note: this does not actually start the timer, this is currently done in the MainViewModel once the user is fully logged in.
            var queueTimerServiceIntent = new Intent(this, typeof(Services.GatewayQueueTimerService));
            StartService(queueTimerServiceIntent);

        }

        protected override Type GetFragmentTypeForViewModel(Type viewModelType)
        {
            return _supportedFragmentViewModels[viewModelType];
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

            SetActivityTitleFromFragment();
        }

        // Current Fragment in the fragment host. Note although the id being used appears to be that of the 
        // original container, it gets replaced during a show by the new fragment *but* keeps its old id.
        public MvxFragment CurrentFragment
        {
            get { return FragmentManager.FindFragmentById(Resource.Id.fragment_host) as MvxFragment; }
        }

 		#endregion

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


        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.main_activity_actions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void SetActivityTitleFromFragment()
        {
            this.ActionBar.Title = ((BaseFragmentViewModel)this.CurrentFragment.DataContext).FragmentTitle;
        }

    }

}