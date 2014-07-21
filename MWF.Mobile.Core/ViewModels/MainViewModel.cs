using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.ViewModels
{

    public class MainViewModel 
		: BaseActivityViewModel
    {

        public MainViewModel(Services.IGatewayQueuedService gatewayQueuedService)
        {
            this.InitialViewModel = new ManifestViewModel();

            // Start the gateway queue timer which will cause submission of any queued data to the MWF Mobile gateway service on a repeat basis
            gatewayQueuedService.StartQueueTimer();
        }

    }

}
