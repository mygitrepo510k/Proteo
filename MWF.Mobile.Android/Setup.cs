using Android.Content;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Android.Portable;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Presentation;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System.Collections.Generic;
using Android.Support;
using Cirrious.CrossCore.IoC;
using System;
using Android.Runtime;

namespace MWF.Mobile.Android
{

    public class Setup : MvxAndroidSetup
    {

        public Setup(Context applicationContext)
            : base(applicationContext)
        {
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception topLevelException = (Exception)e.ExceptionObject;
            PublishExceptionToLog(topLevelException);
        }

     


        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override Cirrious.MvvmCross.Droid.Views.IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var presenter = new Presenters.CustomPresenter();
            Mvx.RegisterSingleton<ICustomPresenter>(presenter);
            return presenter;
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            Mvx.RegisterSingleton<IReachability>(() => new Reachability());
            Mvx.RegisterSingleton<IDeviceInfo>(() => new DeviceInfo());
            Mvx.RegisterSingleton<IToast>(() => new Portable.Toast());
            Mvx.RegisterSingleton<ISound>(() => new Portable.Sound());
            Mvx.RegisterSingleton<IVibrate>(() => new Portable.Vibrate());
            Mvx.RegisterSingleton<ICloseApplication>(() => new Portable.CloseApplication());
            Mvx.RegisterSingleton<ICustomUserInteraction>(() => new Portable.CustomUserInteraction());
            Mvx.RegisterSingleton<ILaunchPhone>(() => new Portable.LaunchPhone());

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override System.Collections.Generic.List<System.Reflection.Assembly> ValueConverterAssemblies
        {
            get
            {
                var toReturn = base.ValueConverterAssemblies;
                toReturn.Add(typeof(Cirrious.MvvmCross.Plugins.Visibility.MvxVisibilityValueConverter).Assembly);
                return toReturn;
            }
        }

        public override void LoadPlugins(Cirrious.CrossCore.Plugins.IMvxPluginManager pluginManager)
        {
            pluginManager.EnsurePluginLoaded<PluginLoader>();
            pluginManager.EnsurePluginLoaded<Cirrious.MvvmCross.Plugins.Visibility.PluginLoader>();
            base.LoadPlugins(pluginManager);
        }

        protected override IList<System.Reflection.Assembly> AndroidViewAssemblies
        {
            get
            {
                var assemblies = base.AndroidViewAssemblies;
                assemblies.Add(typeof(global::Android.Support.V4.Widget.DrawerLayout).Assembly);
                return assemblies;
            }
        }

        private void PublishExceptionToLog(Exception exception)
        {
            Mvx.Resolve<Cirrious.MvvmCross.Plugins.Messenger.IMvxMessenger>().Publish(new MWF.Mobile.Core.Messages.TopLevelExceptionHandlerMessage(this, exception));
        }

    }

}