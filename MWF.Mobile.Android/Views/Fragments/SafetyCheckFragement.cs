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
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Binding.Droid.Views;

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
        private ListView itemList;
        private long selectedItemId;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.SafetyCheckView, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);


            itemList = (ListView)view.FindViewById(Resource.Id.SafetyListView);
            itemList.ItemClick += itemList_ItemClick;
            RegisterForContextMenu(itemList);
        }

        void itemList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ((ListView)sender).ShowContextMenuForChild(e.View);
        }


        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo info)
        {
            AdapterView.AdapterContextMenuInfo menuInfo = (AdapterView.AdapterContextMenuInfo)info;

            selectedItemId = menuInfo.Id;

            menu.SetHeaderTitle("My Menu Header");
            menu.Add(0, (int)MenuOption.Passed, 0, "Pass");
            menu.Add(0, (int)MenuOption.DiscretionaryPass, 0, "Discretionary Pass");
            menu.Add(0, (int)MenuOption.Failed, 0, "Fail");
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var safetyCheckViewModel = (MWF.Mobile.Core.ViewModels.SafetyCheckItemViewModel)((MvxListItemView)((AdapterView.AdapterContextMenuInfo)item.MenuInfo).TargetView).DataContext;

            if (item.ItemId.Equals((int)MenuOption.Passed))
            {
                safetyCheckViewModel.CheckStatus = Core.ViewModels.SafetyCheckEnum.Passed;
            }
            else if (item.ItemId.Equals((int)MenuOption.DiscretionaryPass))
            {
                safetyCheckViewModel.CheckStatus = Core.ViewModels.SafetyCheckEnum.DiscretionaryPass;
            }
            else if (item.ItemId.Equals((int)MenuOption.Failed))
            {
                safetyCheckViewModel.CheckStatus = Core.ViewModels.SafetyCheckEnum.Failed;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}