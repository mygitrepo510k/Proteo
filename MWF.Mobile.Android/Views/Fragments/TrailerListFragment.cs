using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class TrailerListFragment : BaseFragment
    {
        private SearchView _searchView;

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

            
            var searchItem = menu.FindItem(Resource.Id.action_search).ActionView;
            _searchView = searchItem.JavaCast<SearchView>();

            _searchView.QueryTextChange += (s, e) => ((TrailerListViewModel)this.ViewModel).SearchText = e.NewText;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.action_refresh:
                  ((TrailerListViewModel)ViewModel).RefreshListCommand.Execute(null);
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
            this.Activity.ActionBar.Show();
            base.OnViewCreated(view, savedInstanceState);
        }

        
    }

}