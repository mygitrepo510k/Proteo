using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{

    public class TrailerListViewModel
        : BaseTrailerListViewModel
    {


        public TrailerListViewModel(IGatewayService gatewayService, 
                                    IRepositories repositories, 
                                    IReachability reachabibilty, 
                                    IToast toast, 
                                    IStartupService startupService, 
                                    INavigationService navigationService) : base(gatewayService, repositories, reachabibilty, toast, startupService, navigationService)
        {
        }


        #region Protected/Private Methods

        protected override void ConfirmTrailer(Trailer trailer, string title, string message)
        {

            Guid trailerID = Guid.Empty;

            if (trailer != null)
            {
                trailerID = trailer.ID;
            }

            //This will take to the next view model with a trailer value of null.
            Mvx.Resolve<ICustomUserInteraction>().PopUpConfirm(message, async isConfirmed =>
            {
                if (isConfirmed)
                {
                    this.IsBusy = true;

                    await UpdateVehicleListAsync();

                    await UpdateTrailerListAsync();
                    // Try and update safety profiles before continuing
                    await UpdateSafetyProfilesAsync();

                    this.IsBusy = false;

                    _startupService.LoggedInDriver.LastSecondaryVehicleID = trailerID;
                    _startupService.CurrentTrailer = trailer;
                    _navigationService.MoveToNext();

                }
            }, title, "Confirm");
        }



        #endregion
    }
}
