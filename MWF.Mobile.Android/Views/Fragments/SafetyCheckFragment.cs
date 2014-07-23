using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Binding.Droid.Views;

using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{
    enum MenuOption
    {
        Passed = 0,
        DiscretionaryPass = 1,
        Failed = 2
    }

    public class SafetyCheckFragment : MvxFragment
    {
        private ListView _itemList;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.SafetyCheckView, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);


            _itemList = (ListView)view.FindViewById(Resource.Id.SafetyListView);
            _itemList.ItemClick += itemList_ItemClick;
            RegisterForContextMenu(_itemList);

            var checksDoneButton = (Button)view.FindViewById(Resource.Id.checksdonebutton);
            var set = this.CreateBindingSet<SafetyCheckFragment, SafetyCheckViewModel>();
            set.Bind(checksDoneButton).For(b => b.Enabled).To(vm => vm.AllSafetyChecksCompleted);
            set.Apply();
        }

        void itemList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ((ListView)sender).ShowContextMenuForChild(e.View);
        }


        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo info)
        {
            var menuInfo = (AdapterView.AdapterContextMenuInfo)info;
            var safetyCheckItem = ((SafetyCheckItemViewModel)((MvxListItemView)menuInfo.TargetView).DataContext);

            menu.SetHeaderTitle(safetyCheckItem.Title);
            menu.Add(0, (int)MenuOption.Passed, 0, "Pass");
            if (safetyCheckItem.IsDiscreationaryQuestion)
                menu.Add(0, (int)MenuOption.DiscretionaryPass, 0, "Discretionary Pass");
            menu.Add(0, (int)MenuOption.Failed, 0, "Fail");
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var safetyCheckViewModel = (SafetyCheckItemViewModel)((MvxListItemView)((AdapterView.AdapterContextMenuInfo)item.MenuInfo).TargetView).DataContext;

            if (item.ItemId.Equals((int)MenuOption.Passed))
            {
                safetyCheckViewModel.CheckStatus = SafetyCheckEnum.Passed;
            }
            else if (item.ItemId.Equals((int)MenuOption.DiscretionaryPass))
            {
                safetyCheckViewModel.CheckStatus = SafetyCheckEnum.DiscretionaryPass;
                // TODO: Forward to comments screen
            }
            else if (item.ItemId.Equals((int)MenuOption.Failed))
            {
                safetyCheckViewModel.CheckStatus = SafetyCheckEnum.Failed;
                // TODO: Forward to comments screen
            }

            return base.OnOptionsItemSelected(item);
        }

        
    }
}