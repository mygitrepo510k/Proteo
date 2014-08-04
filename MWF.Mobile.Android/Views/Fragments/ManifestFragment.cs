using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class ManifestFragment : BaseFragment
    {
        private IMenu optionsMenu;

        public ManifestViewModel ManifestViewModel
        {
            get { return (ManifestViewModel)ViewModel; }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_Manifest, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {

            // var instructionItem = (TextView)view.FindViewById(Resource.Id.instructionDate);
            //var set = this.CreateBindingSet<ManifestFragment, ManifestViewModel>();
            //set.Bind(instructionItem).For(i => i.Text).To(vm => vm.MobileApplicationData);
            //set.Apply();

            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.manifest_activity_actions, menu);
            base.OnCreateOptionsMenu(menu, inflater);

            this.optionsMenu = menu;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_refresh:
                    SetRefreshActionButtonState(true);
                    ((ManifestViewModel)ViewModel).RefreshListCommand.Execute(null);
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