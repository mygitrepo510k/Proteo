using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Services;
using Chance.MvvmCross.Plugins.UserInteraction;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;

namespace MWF.Mobile.Core.ViewModels
{

    public class InstructionSafetyCheckSignatureViewModel : SafetyCheckSignatureViewModel
    {

        public InstructionSafetyCheckSignatureViewModel(Services.IStartupService startupService, Services.IGatewayQueuedService gatewayQueuedService, IUserInteraction userInteraction, Repositories.IRepositories repositories, INavigationService navigationService)
        {
            _startupService = startupService;
            _gatewayQueuedService = gatewayQueuedService;
            _userInteraction = userInteraction;
            _navigationService = navigationService;
            _repositories = repositories;

        }

        public void Init(NavData<MobileData> navData)
        {
            _navData = navData;
            _navData.Reinflate();

            // Retrieve the vehicle and trailer safety check data 
            // vehicle from start up service, trailer from navData
            var vehicleSafetyCheckData = _startupService.CurrentVehicleSafetyCheckData;
            var trailerSafetyCheckData = _navData.OtherData["UpdatedTrailerSafetyCheckData"] as SafetyCheckData;
            var trailer = _navData.OtherData["UpdatedTrailer"] as Models.Trailer;

            _safetyCheckData = _startupService.GetSafetyCheckData(vehicleSafetyCheckData, trailerSafetyCheckData);

            SafetyProfile safetyProfileVehicle = null;
            SafetyProfile safetyProfileTrailer = null;

            safetyProfileVehicle = _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == _startupService.CurrentVehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
                safetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            if (!_safetyCheckData.Any())
                throw new Exception("Invalid application state - signature screen should not be displayed in cases where there are no safety checks.");

            var combinedOverallStatus = Models.SafetyCheckData.GetOverallStatus(_safetyCheckData.Select(scd => scd.GetOverallStatus()));

            if ((combinedOverallStatus == Enums.SafetyCheckStatus.NotSet) &&
               ((safetyProfileVehicle != null && safetyProfileVehicle.IsVOSACompliant)
               || (safetyProfileTrailer != null && safetyProfileTrailer.IsVOSACompliant)))
                throw new Exception("Cannot proceed to safety check signature screen because the safety check hasn't been completed");

            DriverName = _startupService.LoggedInDriver.DisplayName;
            VehicleRegistration = _startupService.CurrentVehicle.Registration;
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
