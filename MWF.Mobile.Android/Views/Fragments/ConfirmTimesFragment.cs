using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.BindingContext;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Android.Views.Fragments
{
    public class ConfirmTimesFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // MVVMCross fragment boilerplate code
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.Fragment_ConfirmTimes, null);


        }
        

        private TimePicker onSiteTimePicker = null;
        private TimePicker completeTimePicker = null;
        private DatePicker onSiteDatePicker = null;
        private DatePicker completeDatePicker = null;


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var set = this.CreateBindingSet<ConfirmTimesFragment, ConfirmTimesViewModel>();
            
            onSiteTimePicker = view.FindViewById<TimePicker>(Resource.Id.onSiteTimePicker);
            completeTimePicker = view.FindViewById<TimePicker>(Resource.Id.completeTimePicker);
            onSiteDatePicker = view.FindViewById<DatePicker>(Resource.Id.onSiteDatePicker);
            completeDatePicker = view.FindViewById<DatePicker>(Resource.Id.completeDatePicker);

            // Set to 24 Hour mode
            onSiteTimePicker.SetIs24HourView(Java.Lang.Boolean.True);
            completeTimePicker.SetIs24HourView(Java.Lang.Boolean.True);

            // time picker event handlers
            onSiteTimePicker.TimeChanged += _onSiteTimePicker_TimeChanged;
            completeTimePicker.TimeChanged += _completeTimePicker_TimeChanged;

            var onSiteDateTime = ((ConfirmTimesViewModel)this.ViewModel).OnSiteDateTime;

            // binding and Time Pickers just does not work
            onSiteTimePicker.CurrentHour = (Java.Lang.Integer)onSiteDateTime.TimeOfDay.Hours;
            onSiteTimePicker.CurrentMinute = (Java.Lang.Integer)onSiteDateTime.TimeOfDay.Minutes ;

            completeTimePicker.CurrentHour = (Java.Lang.Integer)DateTime.Now.Hour;
            completeTimePicker.CurrentMinute = (Java.Lang.Integer)DateTime.Now.Minute;

            //onSiteDatePicker.UpdateDate(onSiteDateTime.Year, onSiteDateTime.Month, onSiteDateTime.Day);
            onSiteDatePicker.FocusChange += OnSiteDatePicker_FocusChange;
            completeDatePicker.FocusChange += CompleteDatePicker_FocusChange;
            
        }

        private void CompleteDatePicker_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var dateTime = completeDatePicker.DateTime;
            var time = ((ConfirmTimesViewModel)this.ViewModel).CompleteDateTime .TimeOfDay;
            ((ConfirmTimesViewModel)this.ViewModel).CompleteDateTime = dateTime.Date.Add(time);
        }

        private void OnSiteDatePicker_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var dateTime = onSiteDatePicker.DateTime;
            var time = ((ConfirmTimesViewModel)this.ViewModel).OnSiteDateTime.TimeOfDay;
            ((ConfirmTimesViewModel)this.ViewModel).OnSiteDateTime = dateTime.Date.Add(time);
        }

        private void _completeTimePicker_TimeChanged(object sender, TimePicker.TimeChangedEventArgs e)
        {
            var dateTime = completeDatePicker.DateTime;
            var time = new TimeSpan(e.HourOfDay, e.Minute, 0);

            ((ConfirmTimesViewModel)this.ViewModel).CompleteDateTime = dateTime.Date.Add(time);
        }

        private void _onSiteTimePicker_TimeChanged(object sender, TimePicker.TimeChangedEventArgs e)
        {
            var dateTime = onSiteDatePicker.DateTime;
            var time = new TimeSpan(e.HourOfDay, e.Minute, 0);

            ((ConfirmTimesViewModel)this.ViewModel).OnSiteDateTime = dateTime.Date.Add(time); 
        }
    }
    
}