using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{

    //Simple class for in-memory storage of information picked up during the startup process
    public class StartupService
        : MvxNavigatingObject, IStartupService
    {

        private readonly Repositories.IRepositories _repositories = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;

        public StartupService(Repositories.IRepositories repositories, IGatewayQueuedService gatewayQueuedService, IGpsService gpsService)
        {
            _repositories = repositories;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
        }

        public Driver LoggedInDriver { get; set; }
        public SafetyCheckData CurrentVehicleSafetyCheckData { get; set; }
        public SafetyCheckData CurrentTrailerSafetyCheckData { get; set; }
        public Vehicle CurrentVehicle { get; set; }
        public Trailer CurrentTrailer { get; set; }
        public int Mileage { get; set; }

        public IEnumerable<Models.SafetyCheckData> GetCurrentSafetyCheckData()
        {
            var retVal = new List<Models.SafetyCheckData>(2);

            if (this.CurrentVehicleSafetyCheckData != null && this.CurrentVehicleSafetyCheckData.Faults.Any())
                retVal.Add(this.CurrentVehicleSafetyCheckData);

            if (this.CurrentTrailerSafetyCheckData != null && this.CurrentTrailerSafetyCheckData.Faults.Any())
                retVal.Add(this.CurrentTrailerSafetyCheckData);

            return retVal;
        }

        public void Commit()
        {
            // Add the safety checks to the gateway queue
            var safetyCheckData = this.GetCurrentSafetyCheckData();

            var safetyCheckFailed = false;

            if (safetyCheckData.Any())
            {
                var overallStatus = SafetyCheckData.GetOverallStatus(safetyCheckData.Select(scd => scd.GetOverallStatus()));
                safetyCheckFailed = overallStatus == Enums.SafetyCheckStatus.Failed;

                // Store the latest safety check so the driver can display it at a later point if required
                var latestSafetyCheck = new LatestSafetyCheck { DriverID = this.LoggedInDriver.ID };

                if (this.CurrentVehicleSafetyCheckData != null && this.CurrentVehicleSafetyCheckData.Faults.Any())
                    latestSafetyCheck.VehicleSafetyCheck = this.CurrentVehicleSafetyCheckData;

                if (this.CurrentTrailerSafetyCheckData != null && this.CurrentTrailerSafetyCheckData.Faults.Any())
                    latestSafetyCheck.TrailerSafetyCheck = this.CurrentTrailerSafetyCheckData;

                _repositories.LatestSafetyCheckRepository.SetForDriver(latestSafetyCheck);

                // Submit the safety checks to the gateway service
                var smp = _gpsService.GetSmpData(Enums.ReportReason.SMPREASON_SAFETYCHECKREPORT);

                foreach (var safetyCheck in safetyCheckData)
                {
                    // Passed safety check items shouldn't be submitted to the gateway service
                    safetyCheck.Faults.RemoveAll(scf => scf.Status == Enums.SafetyCheckStatus.Passed);

                    // Add the SMP and odometer reading to the safety check
                    safetyCheck.SMP = smp;
                    safetyCheck.Mileage = this.Mileage;
                }

                var actions = safetyCheckData.Select(scd => new Models.GatewayServiceRequest.Action<Models.SafetyCheckData> { Command = "fwSetSafetyCheckData", Data = scd });
                _gatewayQueuedService.AddToQueue(actions);
            }

            //TODO: add the logon event to the gateway queue

            // The startup process is now complete
            if (safetyCheckFailed)
                // The safety check failed so the user should not be logged in
                ChangePresentation(new Presentation.CloseUpToViewPresentationHint<ViewModels.PasscodeViewModel>());
            else
                // Redirect to the main view
                ShowViewModel<ViewModels.MainViewModel>();
        }

    }

}
