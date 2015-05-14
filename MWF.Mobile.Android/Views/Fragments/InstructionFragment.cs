using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Android.Helpers;

namespace MWF.Mobile.Android.Views.Fragments
{

    public class InstructionFragment : BaseFragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_Instruction, null);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var orderListView = (ListView)view.FindViewById(Resource.Id.orderListView);
            // Listview lives inside a scrollview so need to call this helper methods to explicitly
            // force the listview to be the height of its combined children
            ListViewChildMeasurer.SetListViewHeightBasedOnChildren(orderListView);
          
        }


       
    }
}