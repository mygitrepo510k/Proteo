using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class TrailerListFragment : BaseFragment
    {
        private SearchView _searchView;
        private IMenu optionsMenu;
        private BindableProgress _bindableProgress;


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_TrailerListView, null);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.vehicle_activity_actions, menu);
            base.OnCreateOptionsMenu(menu, inflater);

            this.optionsMenu = menu;
            
            var searchItem = menu.FindItem(Resource.Id.action_search).ActionView;
           
            _searchView = searchItem.JavaCast<SearchView>();

            _searchView.QueryTextChange += (s, e) => ((TrailerListViewModel)this.ViewModel).SearchText = e.NewText;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.action_refresh:
                    SetRefreshActionButtonState(true);
                  ((TrailerListViewModel)ViewModel).RefreshListCommand.Execute(null);
                    SetRefreshActionButtonState(false);
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetHasOptionsMenu(true);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            _bindableProgress = new BindableProgress(new ContextThemeWrapper(view.Context, Resource.Style.ProteoDialog));

            this.Activity.ActionBar.Show();
            base.OnViewCreated(view, savedInstanceState);
            var set = this.CreateBindingSet<TrailerListFragment, TrailerListViewModel>();
            set.Bind(_bindableProgress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(_bindableProgress).For(p => p.Message).To(vm => vm.ProgressMessage);
            set.Bind(_bindableProgress).For(p => p.Title).To(vm => vm.ProgressTitle);
            set.Apply();
        }

        public void SetRefreshActionButtonState(bool refreshing)
        {
            if (optionsMenu != null)
            {
                var refreshItem = optionsMenu.FindItem(Resource.Id.action_refresh);
                if (refreshItem != null)
                {
                    if (refreshing)
                    {
                        refreshItem.SetActionView(Resource.Menu.actionbar_indeterminate_progress);
                    }
                    else
                    {
                        refreshItem.SetActionView(null);
                    }
                }
            }
        }
    }

}