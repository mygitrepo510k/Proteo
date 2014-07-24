using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
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
        private IStartupService _startupService;
        private IRepositories _repositories;

        #region Construction

        public SafetyCheckViewModel(IStartupService startupService, IRepositories repositories)
        {
            _startupService = startupService;
            _repositories = repositories;

            Vehicle vehicle = null;
            Trailer trailer = null;

            vehicle = _repositories.VehicleRepository.GetByID(_startupService.LoggedInDriver.LastVehicleID);

            if (_startupService.LoggedInDriver.LastSecondaryVehicleID != Guid.Empty)
                trailer = _repositories.TrailerRepository.GetByID(_startupService.LoggedInDriver.LastSecondaryVehicleID);

            SafetyProfileVehicle = _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == vehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
                SafetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            var allSafetyChecks = new List<SafetyCheckItemViewModel>();

            if (SafetyProfileVehicle != null)
            {
                foreach (var child in SafetyProfileVehicle.Children.OrderBy(spv => spv.Order))
                {
                    var vehicleSafetyCheckFaultTypeView = new SafetyCheckItemViewModel(this)
                    {
                        ID = child.ID,
                        Title = "VEH: " + child.Title,
                        IsDiscreationaryQuestion = child.IsDiscretionaryQuestion
                    };

                    allSafetyChecks.Add(vehicleSafetyCheckFaultTypeView);
                }
            }

            if (SafetyProfileTrailer != null)
            {
                foreach (var child in SafetyProfileTrailer.Children.OrderBy(spt => spt.Order))
                {
                    var trailerSafetyCheckFaultTypeView = new SafetyCheckItemViewModel(this)
                    {
                        ID = child.ID,
                        Title = "TRL: " + child.Title,
                        IsDiscreationaryQuestion = child.IsDiscretionaryQuestion
                    };

                    allSafetyChecks.Add(trailerSafetyCheckFaultTypeView);
                }
            }

            SafetyCheckItemViewModels = allSafetyChecks;

            SetUpSafetyCheckData();

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

        private IList<SafetyCheckItemViewModel> _safetyCheckItemViewModels;
        public IList<SafetyCheckItemViewModel> SafetyCheckItemViewModels
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

        private MvxCommand _logFaultCommand;
        public System.Windows.Input.ICommand LogFaultCommand
        {
            get
            {
                _logFaultCommand = _logFaultCommand ?? new MvxCommand(DoLogFaultCommand);
                return _logFaultCommand;
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
            if (SafetyProfileVehicle.OdometerRequired)
                ShowViewModel<OdometerViewModel>();
            else
                ShowViewModel<SafetyCheckSignatureViewModel>();
        }

        public override string FragmentTitle
        {
            get { return "Safety Check"; }
        }

        public async Task<bool> OnBackButtonPressed()
        {
            return await Mvx.Resolve<IUserInteraction>().ConfirmAsync("All changes will be lost, do you wish to continue?", "Changes will be lost!");
        }


        private void DoLogFaultCommand()
        {

        }

        
        //Sets up the safety check data in the startup info service
        //This is the result of the safety check which will be sent to bluesphere
        //and persisted locally in case the safety check needs to be shown by the driver
        private void SetUpSafetyCheckData()
        {
            //TODO: make sure all of the safety check data fields are populated
            SafetyCheckData vehicleSafetyCheckData = new SafetyCheckData() { ID = Guid.NewGuid(), Faults = new List<SafetyCheckFault>()};
            SafetyCheckData trailerSafetyCheckData = new SafetyCheckData() { ID = Guid.NewGuid(), Faults = new List<SafetyCheckFault>() };


            //create a safety fault for every item in the visual list (we'll remove the passes later)
            foreach (var item in SafetyCheckItemViewModels)
            {
                SafetyCheckFault safetyCheckFault = new SafetyCheckFault() { ID = Guid.NewGuid(), Title = item.Title, FaultTypeID = item.ID };


                if (item.Title.StartsWith("VEH"))
                {
                    safetyCheckFault.SafetyCheckDataID = SafetyProfileVehicle.ID;
                    vehicleSafetyCheckData.Faults.Add(safetyCheckFault);
                }
                else
                {
                    safetyCheckFault.SafetyCheckDataID = SafetyProfileTrailer.ID;
                    trailerSafetyCheckData.Faults.Add(safetyCheckFault);
                }

                item.SafetyCheckFault = safetyCheckFault;

            }

            _startupService.CurrentVehicleSafetyCheckData = vehicleSafetyCheckData;
            _startupService.CurrentTrailerSafetyCheckData = trailerSafetyCheckData;

        }

        #endregion

    }
}
