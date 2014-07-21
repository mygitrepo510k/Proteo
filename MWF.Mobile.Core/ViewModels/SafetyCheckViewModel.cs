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

namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckViewModel : MvxViewModel
    {
        private IStartupInfoService _startupInfoService;
        private IRepositories _repositories;

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

        public string DoneButtonLabel
        {
            get { return "Done"; }
        }

        private MvxCommand _doneCheckCommand;
        public System.Windows.Input.ICommand DoneCheckCommand
        {
            get { return (_doneCheckCommand = _doneCheckCommand ?? new MvxCommand(async () => await Mvx.Resolve<IUserInteraction>().AlertAsync("Safety Check Complete"))); }
        }

        private bool _allSafetyChecksComplete;
        public bool AllSafetyChecksComplete
        {
            get 
            {
                bool allChecksPassed = true;
                foreach (var safetyCheckItem in SafetyCheckItemViewModels)
                {
                    if (!allChecksPassed) 
                        return allChecksPassed;

                    allChecksPassed = (safetyCheckItem.CheckStatus == SafetyCheckEnum.Passed || safetyCheckItem.CheckStatus == SafetyCheckEnum.DiscretionaryPass);
                }

                return allChecksPassed;
            }
            set 
            {
                _allSafetyChecksComplete = value; 
                RaisePropertyChanged(() => AllSafetyChecksComplete);
            }
        }

        public void CheckSafetyCheckItemsStatus()
        {
            RaisePropertyChanged(() => AllSafetyChecksComplete);
        }

        public SafetyCheckViewModel(IStartupInfoService startupInfoService, IRepositories repositories)
        {
            _startupInfoService = startupInfoService;
            _repositories = repositories;

            Vehicle vehicle = null;
            Trailer trailer = null;
            
            vehicle = _repositories.VehicleRepository.GetByID(_startupInfoService.LoggedInDriver.LastVehicleID);
            
            if (_startupInfoService.LoggedInDriver.LastSecondaryVehicleID != Guid.Empty)
                trailer = _repositories.TrailerRepository.GetByID(_startupInfoService.LoggedInDriver.LastSecondaryVehicleID);

            SafetyProfileVehicle = _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == vehicle.SafetyCheckProfileIntLink).SingleOrDefault();

            if (trailer != null)
                SafetyProfileTrailer = _repositories.SafetyProfileRepository.GetAll().Where(spt => spt.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault();

            var allSafetyChecks = new List<SafetyCheckItemViewModel>();

            if (SafetyProfileVehicle != null)
            {
                foreach (var child in SafetyProfileVehicle.Children.OrderBy(spv => spv.Order))
                {
                    var vehicleSafetyCheckFaultTypeView = new SafetyCheckItemViewModel(this) { 
                        ID = child.ID,
                        Title = "VEH: " + child.Title,
                        CheckStatus = SafetyCheckEnum.NotSet,
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
                        CheckStatus = SafetyCheckEnum.NotSet,
                        IsDiscreationaryQuestion = child.IsDiscretionaryQuestion
                    };

                    allSafetyChecks.Add(trailerSafetyCheckFaultTypeView);
                }
            }            

            SafetyCheckItemViewModels = allSafetyChecks;
        }
    }
}
