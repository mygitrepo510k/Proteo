﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionSafetyCheckViewModel : SafetyCheckViewModel
    {

        #region private fields


        #endregion

        #region Construction

        public InstructionSafetyCheckViewModel(IStartupService startupService, INavigationService navigationService, IRepositories repositories)
        {
            _startupService = startupService;
            _repositories = repositories;
            _navigationService = navigationService;
          
        }



        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();

            Models.Trailer trailer = _navData.OtherData["UpdatedTrailer"] as Models.Trailer;

            if (trailer != null)
                SafetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            this.SafetyCheckItemViewModels = new List<SafetyCheckItemViewModel>();
            _navData.OtherData["UpdatedTrailerSafetyCheckData"] = null;

            if (SafetyProfileTrailer != null)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileTrailer, _startupService.LoggedInDriver, trailer, true);
                _navData.OtherData["UpdatedTrailerSafetyCheckData"] = safetyCheckData;
            }

            if (_navData.OtherData["UpdatedTrailerSafetyCheckData"] == null)
            {
                Mvx.Resolve<ICustomUserInteraction>().PopUpAlert("A safety check profile for your trailer has not been found - Perform a manual safety check.", () => { _navigationService.MoveToNext(_navData); });
            }

        }
        #endregion

    }
}