using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseTrailerListViewModel
        : BaseFragmentViewModel
    {

        #region Protected Members

        protected IEnumerable<Trailer> _originalTrailerList;
        protected readonly IRepositories _repositories;
        protected readonly IGatewayService _gatewayService;
        protected readonly IReachability _reachability;
        protected readonly IStartupService _startupService;
        protected readonly INavigationService _navigationService;
        protected readonly IToast _toast;

        #endregion 

        #region Construction

        public BaseTrailerListViewModel(IGatewayService gatewayService, IRepositories repositories, IReachability reachabibilty, IToast toast, IStartupService startupService, INavigationService navigationService)
        {
            _reachability = reachabibilty;
            _startupService = startupService;
            _navigationService = navigationService;
            _gatewayService = gatewayService;

            _repositories = repositories;
            Trailers = _originalTrailerList = _repositories.TrailerRepository.GetAll();
        }

        #endregion

        #region Public Properties

        public override string FragmentTitle
        {
            get { return "Trailer"; }
        }

        public string TrailerButtonLabel
        {
            get { return "No trailer"; }
        }

        public string TrailerSelectText
        {
            get { return "Select trailer for " + VehicleRegistration + " - Showing " + Trailers.ToList().Count + " of " + _originalTrailerList.ToList().Count; }
        }


        public String VehicleRegistration
        {
            get
            {
                return _repositories.VehicleRepository.GetByID(_startupService.LoggedInDriver.LastVehicleID).Registration;
            }
        }

        private IEnumerable<Trailer> _trailers;
        public IEnumerable<Trailer> Trailers
        {
            get { return _trailers; }
            set { _trailers = value; RaisePropertyChanged(() => Trailers); }
        }

        //This is method associated with the search button in the action bar.
        private string _trailerSearchText;
        public string TrailerSearchText
        {
            get { return _trailerSearchText; }
            set { _trailerSearchText = value; FilterList(); RaisePropertyChanged(() => TrailerSelectText); }
        }


        private void FilterList()
        {
            if (string.IsNullOrEmpty(TrailerSearchText))
            {
                Trailers = _originalTrailerList;
            }
            else
            {
                Trailers = _originalTrailerList.Where(t => t.Registration != null && t.Registration.ToUpper().Contains(TrailerSearchText.ToUpper()));
            }
        }

        //This is method associated with the refresh button in the action bar. 
        private MvxCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get
            {
                return (_refreshListCommand = _refreshListCommand ?? new MvxCommand(async () => await UpdateTrailerListAsync()));
            }
        }

        private MvxCommand<Trailer> _trailerSelectCommand;
        public ICommand TrailerSelectCommand
        {
            get
            {
                var title = "Confirm your trailer";
                return (_trailerSelectCommand = _trailerSelectCommand ?? new MvxCommand<Trailer>(t => ConfirmTrailer(t, title, t.Registration)));
            }
        }


        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string ProgressTitle
        {
            get { return "Downloading data..."; }
        }

        public string ProgressMessage
        {
            get { return "Setting up safety check profile..."; }
        }

        private MvxCommand<Trailer> _notrailerSelectCommand;
        public ICommand NoTrailerSelectCommand
        {
            get
            {
                var title = "No trailer";
                var message = "Confirm you don't have a trailer";
                return (_notrailerSelectCommand = _notrailerSelectCommand ?? new MvxCommand<Trailer>(t => ConfirmTrailer(null, title, message)));
            }
        }


        #endregion

        #region Protected Methods

        protected abstract void ConfirmTrailer(Trailer trailer, string title, string message);

        protected async Task UpdateTrailerListAsync()
        {

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
            {
                var vehicleViews = await _gatewayService.GetVehicleViews();

                var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
                }

                var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
                var trailers = vehiclesAndTrailers.Where(bv => bv.IsTrailer).Select(bv => new Models.Trailer(bv));

                if (trailers != null)
                {

                    _repositories.TrailerRepository.DeleteAll();

                    _repositories.TrailerRepository.Insert(trailers);

                    Trailers = _originalTrailerList = _repositories.TrailerRepository.GetAll();

                    //Recalls the filter text if there is text in the search field.
                    if (TrailerSearchText != null)
                    {
                        FilterList();
                    }
                }
            }
        }

        protected async Task UpdateSafetyProfilesAsync()
        {
            // First check if we have a internet connection. If we do go and get the latest safety checks from Blue Sphere.
            if (_reachability.IsConnected())
            {
                var safetyProfiles = await _gatewayService.GetSafetyProfiles();

                if (safetyProfiles != null)
                {
                    _repositories.SafetyProfileRepository.DeleteAll();
                    _repositories.SafetyProfileRepository.Insert(safetyProfiles);
                }
            }
            else
            {
                var safetyProfileRepository = _repositories.SafetyProfileRepository;
                if (safetyProfileRepository.GetAll().ToList().Count == 0)
                {
                    Mvx.Resolve<IUserInteraction>().Alert("No Profiles Found.");
                }
            }
        }

        protected async Task UpdateVehicleListAsync()
        {

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
            }
            else
            {
                var vehicleViews = await _gatewayService.GetVehicleViews();

                var vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
                }

                var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
                var vehicles = vehiclesAndTrailers.Where(bv => !bv.IsTrailer).Select(bv => new Models.Vehicle(bv));

                if (vehicles != null)
                {
                    _repositories.VehicleRepository.DeleteAll();
                    _repositories.VehicleRepository.Insert(vehicles);

                }
            }
        }

        #endregion

    }

}
