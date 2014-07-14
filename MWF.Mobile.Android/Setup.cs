using Android.Content;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Android.Portable;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Android
{

    public class Setup : MvxAndroidSetup
    {

        public Setup(Context applicationContext) : base(applicationContext)
        {
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
            Mvx.RegisterSingleton<Presenters.ICustomPresenter>(presenter);
            return presenter;
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            IDataService dataService = Mvx.Resolve<IDataService>();

            Mvx.RegisterSingleton<IReachability>(() => new Reachability());


        }

    }

}