using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class MainViewModel 
		: MvxViewModel
    {

        public void Init()
        {
            ShowViewModel<ManifestViewModel>();
        }

    }

}
