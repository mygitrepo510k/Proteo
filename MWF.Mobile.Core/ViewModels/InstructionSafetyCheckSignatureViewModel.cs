using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public class InstructionSafetyCheckSignatureViewModel : SafetyCheckSignatureViewModel
    {

        public InstructionSafetyCheckSignatureViewModel(Services.IInfoService infoService, Services.IGatewayQueuedService gatewayQueuedService, ICustomUserInteraction userInteraction, Repositories.IRepositories repositories, INavigationService navigationService, ISafetyCheckService safetyCheckService)
        {
            _infoService = infoService;
            _gatewayQueuedService = gatewayQueuedService;
            _userInteraction = userInteraction;
            _navigationService = navigationService;
            _repositories = repositories;
            _safetyCheckService = safetyCheckService;

        }

        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();

            // Retrieve the vehicle and trailer safety check data 
            // vehicle from start up service, trailer from navData
            var vehicleSafetyCheckData = _safetyCheckService.CurrentVehicleSafetyCheckData;
            var trailerSafetyCheckData = _navData.OtherData["UpdatedTrailerSafetyCheckData"] as SafetyCheckData;
            var trailer = _navData.OtherData["UpdatedTrailer"] as Models.Trailer;

            _safetyCheckData = _safetyCheckService.GetSafetyCheckData(vehicleSafetyCheckData, trailerSafetyCheckData);

            SafetyProfile safetyProfileVehicle = null;
            SafetyProfile safetyProfileTrailer = null;

            safetyProfileVehicle = _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == _infoService.CurrentVehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
                safetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            if (!_safetyCheckData.Any())
                throw new Exception("Invalid application state - signature screen should not be displayed in cases where there are no safety checks.");

            var combinedOverallStatus = Models.SafetyCheckData.GetOverallStatus(_safetyCheckData.Select(scd => scd.GetOverallStatus()));

            if ((combinedOverallStatus == Enums.SafetyCheckStatus.NotSet) &&
               ((safetyProfileVehicle != null && safetyProfileVehicle.IsVOSACompliant)
               || (safetyProfileTrailer != null && safetyProfileTrailer.IsVOSACompliant)))
                throw new Exception("Cannot proceed to safety check signature screen because the safety check hasn't been completed");

            DriverName = _infoService.LoggedInDriver.DisplayName;
            VehicleRegistration = _infoService.CurrentVehicle.Registration;
            TrailerRef = trailer == null ? "- no trailer -" : trailer.Registration;

            var config = _repositories.ConfigRepository.Get();
            if ((safetyProfileVehicle != null && safetyProfileVehicle.IsVOSACompliant)
               || (safetyProfileTrailer != null && safetyProfileTrailer.IsVOSACompliant))
            {

                switch (combinedOverallStatus)
                {
                    case Enums.SafetyCheckStatus.Failed:
                        this.ConfirmationText = config.SafetyCheckFailText;
                        break;
                    case Enums.SafetyCheckStatus.DiscretionaryPass:
                        this.ConfirmationText = config.SafetyCheckDiscretionaryText;
                        break;
                    case Enums.SafetyCheckStatus.Passed:
                        this.ConfirmationText = config.SafetyCheckPassText;
                        break;
                    default:
                        throw new Exception("Unexpected safety check status");
                }
            }
            else
            {
                this.ConfirmationText = config.SafetyCheckPassText;
            }

        }

      
    }

}
