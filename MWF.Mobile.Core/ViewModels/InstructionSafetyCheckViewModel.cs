using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionSafetyCheckViewModel : SafetyCheckViewModel
    {

        #region Construction

        public InstructionSafetyCheckViewModel(IInfoService infoService, INavigationService navigationService, IRepositories repositories, ISafetyCheckService safetyCheckService)
            : base(infoService, navigationService, repositories, safetyCheckService)
        {
        }

        public override Task Init()
        {
            // Don't call down to base Init method: what it does is not relevant for instruction safety checks, there is corresponding code specifically for instructions in Init(navData) below.
            return Task.FromResult(0);
        }

        public async Task Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();

            Models.Trailer trailer = _navData.OtherData["UpdatedTrailer"] as Models.Trailer;

            if (trailer != null)
            {
                var safetyProfileData = await _repositories.SafetyProfileRepository.GetAllAsync();
                SafetyProfileTrailer = safetyProfileData.Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();
            }

            this.SafetyCheckItemViewModels = new List<SafetyCheckItemViewModel>();
            _navData.OtherData["UpdatedTrailerSafetyCheckData"] = null;

            if (SafetyProfileTrailer != null)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileTrailer, _infoService.LoggedInDriver, trailer, true);
                _navData.OtherData["UpdatedTrailerSafetyCheckData"] = safetyCheckData;
            }

            if (_navData.OtherData["UpdatedTrailerSafetyCheckData"] == null)
            {
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("A safety check profile for your trailer has not been found - Perform a manual safety check.");
                await _navigationService.MoveToNextAsync(_navData);
            }

        }

        #endregion

    }

}
