using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseActivityViewModel 
		: MvxViewModel
    {

        public IMvxViewModel InitialViewModel { get; protected set; }

    }

}
