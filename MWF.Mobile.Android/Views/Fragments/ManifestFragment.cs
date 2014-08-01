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

namespace MWF.Mobile.Android.Views.Fragments
{

    public class ManifestFragment : BaseFragment
    {

        public Core.ViewModels.ManifestViewModel ManifestViewModel
        {
            get { return (Core.ViewModels.ManifestViewModel)ViewModel; }
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
        
    }

}