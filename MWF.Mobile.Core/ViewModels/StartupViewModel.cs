using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class StartupViewModel 
		: MvxViewModel
    {

        public void Init()
        {
            ShowViewModel<PasscodeViewModel>();
        }

    }

}
