using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
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
    public class SafetyCheckViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        private readonly IStartupService _startupService;
        private readonly IRepositories _repositories;
        private readonly INavigationService _navigationService;

        #region Construction

        public SafetyCheckViewModel(IStartupService startupService, INavigationService navigationService, IRepositories repositories)
        {
            _startupService = startupService;
            _repositories = repositories;
            _navigationService = navigationService;

            Vehicle vehicle = null;
            Trailer trailer = null;

            vehicle = _repositories.VehicleRepository.GetByID(_startupService.LoggedInDriver.LastVehicleID);
            _startupService.CurrentVehicle = vehicle;

            if (_startupService.LoggedInDriver.LastSecondaryVehicleID != Guid.Empty)
            {
                trailer = _repositories.TrailerRepository.GetByID(_startupService.LoggedInDriver.LastSecondaryVehicleID);
                _startupService.CurrentTrailer = trailer;
            }

            SafetyProfileVehicle = _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == vehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
                SafetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            this.SafetyCheckItemViewModels = new List<SafetyCheckItemViewModel>();

            _startupService.CurrentVehicleSafetyCheckData = null;
            _startupService.CurrentTrailerSafetyCheckData = null;

            if (SafetyProfileVehicle != null && SafetyProfileVehicle.DisplayAtLogon)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileVehicle, _startupService.LoggedInDriver, _startupService.CurrentVehicle, false);
                _startupService.CurrentVehicleSafetyCheckData = safetyCheckData;
            }

            if (SafetyProfileTrailer != null && SafetyProfileTrailer.DisplayAtLogon)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileTrailer, _startupService.LoggedInDriver, _startupService.CurrentTrailer, true);
                _startupService.CurrentTrailerSafetyCheckData = safetyCheckData;
            }

            if (_startupService.CurrentTrailerSafetyCheckData == null && _startupService.CurrentVehicleSafetyCheckData == null)
            {
                Mvx.Resolve<IUserInteraction>().Alert("A safety check profile for your vehicle and/or trailer has not been found - Perform a manual safety check.", () => { startupService.Commit(); _navigationService.MoveToNext(); });
            }
        }

        private SafetyCheckData GenerateSafetyCheckData(SafetyProfile safetyProfile, Driver driver, BaseVehicle vehicle, bool isTrailer)
        {
            var faults = safetyProfile.Children
                .OrderBy(scft => scft.Order)
                .Select(scft => new Models.SafetyCheckFault
                {
                    SafetyCheckDataID = safetyProfile.ID,
                    Title = scft.Title,
                    FaultTypeID = scft.ID,
                    IsDiscretionaryQuestion = scft.IsDiscretionaryQuestion,
                })
                .ToList();

            // Set up the safety check data to be set in the startup service.
            // This is the result of the safety check which will be sent to bluesphere
            // and persisted locally in case the safety check needs to be shown by the driver.
            var safetyCheckData = new SafetyCheckData
            {
                ProfileIntLink = safetyProfile.IntLink,
                DriverID = driver.ID,
                DriverTitle = driver.Title,
                VehicleID = vehicle.ID,
                VehicleRegistration = vehicle.Registration,
                Faults = faults,
            };

            // Add the safety check item view models
            this.SafetyCheckItemViewModels.AddRange(faults.Select(scf => new SafetyCheckItemViewModel(this)
            {
                ID = scf.ID,
                SafetyCheckFault = scf,
                IsVehicle = !isTrailer,
                Title = (isTrailer ? "TRL: " : "VEH: ") + scf.Title,
                IsDiscretionaryQuestion = scf.IsDiscretionaryQuestion,
            }));

            return safetyCheckData;
        }

        #endregion

        #region Public Properties

        private SafetyProfile _safetyProfileVehicle;
        public SafetyProfile SafetyProfileVehicle
        {
            get { return _safetyProfileVehicle; }
            set { _safetyProfileVehicle = value; }
        }

        private SafetyProfile _safetyProfileTrailer;
        public SafetyProfile SafetyProfileTrailer
        {
            get { return _safetyProfileTrailer; }
            set { _safetyProfileTrailer = value; }
        }

        private List<SafetyCheckItemViewModel> _safetyCheckItemViewModels;
        public List<SafetyCheckItemViewModel> SafetyCheckItemViewModels
        {
            get { return _safetyCheckItemViewModels; }
            set { _safetyCheckItemViewModels = value; RaisePropertyChanged(() => SafetyCheckItemViewModels); }
        }

        public string ChecksDoneButtonLabel
        {
            get { return "Done"; }
        }

        private MvxCommand _checksDoneCommand;
        public System.Windows.Input.ICommand ChecksDoneCommand
        {
            get 
            { 
                _checksDoneCommand = _checksDoneCommand ?? new MvxCommand(DoChecksDoneCommand); 
                return _checksDoneCommand;
            }
        }

        public bool AllSafetyChecksCompleted
        {
            get 
            {
                bool allChecksCompleted = true;
                foreach (var safetyCheckItem in SafetyCheckItemViewModels)
                {
                    if (!allChecksCompleted)
                        return allChecksCompleted;

                    allChecksCompleted = (safetyCheckItem.CheckStatus != Enums.SafetyCheckStatus.NotSet);
                }

                return allChecksCompleted;
            }
            set 
            {
                RaisePropertyChanged(() => AllSafetyChecksCompleted);
            }
        }

        public void CheckSafetyCheckItemsStatus()
        {
            RaisePropertyChanged(() => AllSafetyChecksCompleted);
        }

        #endregion

        #region Private Methods

        private void DoChecksDoneCommand()
        {
            _navigationService.MoveToNext();
        }

        public override string FragmentTitle
        {
            get { return "Safety checklist"; }
        }

        public async Task<bool> OnBackButtonPressed()
        {
            return await Mvx.Resolve<IUserInteraction>().ConfirmAsync("All information you have entered will be lost, do you wish to continue?", "Abandon safety check!", "Continue");
        }

        #endregion

    }
}
