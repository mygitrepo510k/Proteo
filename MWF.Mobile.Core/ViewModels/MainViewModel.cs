using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class MainViewModel 
		: BaseActivityViewModel
    {

        public MainViewModel()
        {
            this.InitialViewModel = new ManifestViewModel();
        }

    }

}
