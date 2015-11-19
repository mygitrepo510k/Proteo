using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckViewModel : BaseFragmentViewModel, IBackButtonHandler
    {
        #region private/protected members

        protected IInfoService _infoService;
        protected IRepositories _repositories;
        protected INavigationService _navigationService;
        protected NavData<MobileData> _navData;
        protected ISafetyCheckService _safetyCheckService;

        #endregion

        #region Construction

        public SafetyCheckViewModel(IInfoService infoService, INavigationService navigationService, IRepositories repositories, ISafetyCheckService safetyCheckService)
        {
            _infoService = infoService;
            _repositories = repositories;
            _navigationService = navigationService;
            _safetyCheckService = safetyCheckService;

           
        }
        public async override void Start()
        {
            base.Start();
            Models.Vehicle vehicle = null;
            Models.Trailer trailer = null;

            vehicle = await _repositories.VehicleRepository.GetByIDAsync(_infoService.LoggedInDriver.LastVehicleID);
            _infoService.CurrentVehicle = vehicle;

            if (_infoService.LoggedInDriver.LastSecondaryVehicleID != Guid.Empty)
            {
                trailer = await _repositories.TrailerRepository.GetByIDAsync(_infoService.LoggedInDriver.LastSecondaryVehicleID);
                _infoService.CurrentTrailer = trailer;
            }

            var safetyProfileVehicleData = await _repositories.SafetyProfileRepository.GetAllAsync();
            SafetyProfileVehicle = safetyProfileVehicleData.Where(spv => spv.IntLink == vehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
            {
                var safetyProfileTrailerData = await _repositories.SafetyProfileRepository.GetAllAsync();
                SafetyProfileTrailer = safetyProfileVehicleData.Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();
            }

            this.SafetyCheckItemViewModels = new List<SafetyCheckItemViewModel>();

            _safetyCheckService.CurrentVehicleSafetyCheckData = null;
            _safetyCheckService.CurrentTrailerSafetyCheckData = null;

            if (SafetyProfileVehicle != null && SafetyProfileVehicle.DisplayAtLogon)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileVehicle, _infoService.LoggedInDriver, _infoService.CurrentVehicle, false);
                _safetyCheckService.CurrentVehicleSafetyCheckData = safetyCheckData;
            }

            if (SafetyProfileTrailer != null && SafetyProfileTrailer.DisplayAtLogon)
            {
                var safetyCheckData = GenerateSafetyCheckData(SafetyProfileTrailer, _infoService.LoggedInDriver, _infoService.CurrentTrailer, true);
                _safetyCheckService.CurrentTrailerSafetyCheckData = safetyCheckData;
            }

            if (_safetyCheckService.CurrentTrailerSafetyCheckData == null && _safetyCheckService.CurrentVehicleSafetyCheckData == null)
            {
                Mvx.Resolve<ICustomUserInteraction>().Alert("A safety check profile for your vehicle and/or trailer has not been found - Perform a manual safety check.", () => { _navigationService.MoveToNext(); _safetyCheckService.CommitSafetyCheckData(); });
            }
        }

        public SafetyCheckViewModel()
        {
        }

        protected SafetyCheckData GenerateSafetyCheckData(SafetyProfile safetyProfile, Driver driver, BaseVehicle vehicle, bool isTrailer)
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
                IsTrailer = isTrailer
            };

            // Add the safety check item view models
            this.SafetyCheckItemViewModels.AddRange(faults.Select(scf => new SafetyCheckItemViewModel(this, _navigationService)
            {
                ID = scf.ID,
                SafetyCheckFault = scf,
                IsVehicle = !isTrailer,
                Title = (isTrailer ? "TRL: " : "VEH: ") + scf.Title,
                IsDiscretionaryQuestion = scf.IsDiscretionaryQuestion,
                CheckStatus = scf.Status,
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

        public bool CanSafetyChecksBeCompleted
        {
            get
            {
                bool allChecksCompleted = true;

                if ((this.SafetyProfileVehicle != null && this.SafetyProfileVehicle.IsVOSACompliant)
                    || (this.SafetyProfileTrailer != null && this.SafetyProfileTrailer.IsVOSACompliant))
                {
                    foreach (var safetyCheckItem in SafetyCheckItemViewModels)
                    {
                        if (!allChecksCompleted)
                            return allChecksCompleted;

                        allChecksCompleted = (safetyCheckItem.CheckStatus != Enums.SafetyCheckStatus.NotSet);
                    }
                }

                return allChecksCompleted;
            }
            set
            {
                RaisePropertyChanged(() => CanSafetyChecksBeCompleted);
            }
        }

        public void CheckSafetyCheckItemsStatus()
        {
            RaisePropertyChanged(() => CanSafetyChecksBeCompleted);
        }

        #endregion

        #region Private Methods

        private void DoChecksDoneCommand()
        {
            _navigationService.MoveToNext(_navData);
        }

        public override string FragmentTitle
        {
            get { return "Safety checklist"; }
        }



        public async Task<bool> OnBackButtonPressed()
        {
            bool continueWithBackPress = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("All information you have entered will be lost, do you wish to continue?", "Abandon safety check!", "Continue");

            if (continueWithBackPress)
            {
                if (_navigationService.IsBackActionDefined())
                {

                    _navigationService.GoBack();
                    return false;
                }

                return true;
            }

            return false;

        }

        #endregion

    }
}
