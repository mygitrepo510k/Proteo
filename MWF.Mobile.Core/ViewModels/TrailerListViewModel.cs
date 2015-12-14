﻿using System;
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
        private readonly IApplicationProfileRepository _applicationProfileRepository;

        public TrailerListViewModel(IGatewayService gatewayService, 
                                    IRepositories repositories, 
                                    IReachability reachabibilty, 
                                    IToast toast, 
                                    IInfoService infoService, 
                                    INavigationService navigationService) : base(gatewayService, repositories, reachabibilty, toast, infoService, navigationService)
        {
            _applicationProfileRepository = repositories.ApplicationRepository;
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
                    // we only need to check profiles once a day
                    var profileData = await _applicationProfileRepository.GetAllAsync();
                    if (profileData.Count() == 0)
                    {
                        // we have seen an issue where there was no profile so lets cater for this now.
                        var profile = await _gatewayService.GetApplicationProfileAsync();
                        await _applicationProfileRepository.InsertAsync(profile);

                    }
                    var applicationProfile = profileData.OrderByDescending(x=> x.IntLink).First();
                    if (DateTime.Now.Subtract( applicationProfile.LastVehicleAndDriverSync).TotalHours > 23)
                    {
                        await UpdateVehicleListAsync();
                        await UpdateTrailerListAsync();
                        // Try and update safety profiles before continuing
                        await UpdateSafetyProfilesAsync();
                        ProgressMessage = "Updating Application Profile.";
                        this.IsBusy = true;
                        applicationProfile = await _gatewayService.GetApplicationProfileAsync();
                        applicationProfile.LastVehicleAndDriverSync = DateTime.Now;
                        await _applicationProfileRepository.UpdateAsync(applicationProfile);
                        
                    }
                }
                finally
                {
                    this.IsBusy = false;
                }

                _infoService.LoggedInDriver.LastSecondaryVehicleID = trailerID;
                _infoService.CurrentTrailer = trailer;
                await _navigationService.MoveToNextAsync();
            }
        }

        #endregion
    }
}
