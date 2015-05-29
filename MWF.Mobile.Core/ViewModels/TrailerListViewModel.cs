using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class TrailerListViewModel
        : BaseTrailerListViewModel
    {


        public TrailerListViewModel(IGatewayService gatewayService, 
                                    IRepositories repositories, 
                                    IReachability reachabibilty, 
                                    IToast toast, 
                                    IInfoService infoService, 
                                    INavigationService navigationService) : base(gatewayService, repositories, reachabibilty, toast, infoService, navigationService)
        {
        }

        #region Protected/Private Methods

        protected override async Task ConfirmTrailerAsync(Trailer trailer, string title, string message)
        {
            Guid trailerID = Guid.Empty;

            if (trailer != null)
                trailerID = trailer.ID;

            //This will take to the next view model with a trailer value of null.
            if (await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(message, title, "Confirm"))
            {
                this.IsBusy = true;

                try
                {
                    await UpdateVehicleListAsync();
                    await UpdateTrailerListAsync();

                    // Try and update safety profiles before continuing
                    await UpdateSafetyProfilesAsync();
                }
                finally
                {
                    this.IsBusy = false;
                }

                _infoService.LoggedInDriver.LastSecondaryVehicleID = trailerID;
                _infoService.CurrentTrailer = trailer;
                _navigationService.MoveToNext();
            }
        }

        #endregion
    }
}
