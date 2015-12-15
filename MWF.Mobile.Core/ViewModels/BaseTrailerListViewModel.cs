using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;


namespace MWF.Mobile.Core.ViewModels
{

    public abstract class BaseTrailerListViewModel
        : BaseFragmentViewModel
    {

        #region Protected Members

        protected IEnumerable<TrailerItemViewModel> _originalTrailerList;
        protected readonly IRepositories _repositories;
        protected readonly IGatewayService _gatewayService;
        protected readonly IReachability _reachability;
        protected readonly IInfoService _infoService;
        protected readonly INavigationService _navigationService;
        protected readonly IToast _toast;

        #endregion 

        #region Construction

        public BaseTrailerListViewModel(IGatewayService gatewayService, IRepositories repositories, IReachability reachabilty, IToast toast, IInfoService infoService, INavigationService navigationService)
        {
            _reachability = reachabilty;
            _toast = toast;
            _infoService = infoService;
            _navigationService = navigationService;
            _gatewayService = gatewayService;
            _repositories = repositories;
            
            ProgressMessage = "Updating Trailers.";
        }

        public async Task Init()
        {
            base.Start();

            var _vehicle = await _repositories.VehicleRepository.GetByIDAsync(_infoService.LoggedInDriver.LastVehicleID);

            if (_vehicle != null)
                this.VehicleRegistration = _vehicle.Registration;

            await this.GetTrailerModelsAsync();
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

        private string _trailerSelectText = "";
        public string TrailerSelectText
        {
            get { return _trailerSelectText; }
            set { _trailerSelectText = value; RaisePropertyChanged(() => TrailerSelectText); }
        }

        private string _vehicleRegistration = "";
        public string VehicleRegistration
        {
            get { return _vehicleRegistration; }
            set { _vehicleRegistration = value; RaisePropertyChanged(() => VehicleRegistration); }
        }

        private IEnumerable<TrailerItemViewModel> _trailers;
        public IEnumerable<TrailerItemViewModel> Trailers
        {
            get { return _trailers; }
            set 
            { 
                _trailers = value;
                RaisePropertyChanged(() => Trailers); 
            }
        }

        //This is method associated with the search button in the action bar.
        private string _trailerSearchText;
        public string TrailerSearchText
        {
            get { return _trailerSearchText; }
            set { _trailerSearchText = value; this.FilterList(); RaisePropertyChanged(() => TrailerSearchText); }
        }

        private void FilterList()
        {
            if (_originalTrailerList != null)
            {
                if (string.IsNullOrEmpty(TrailerSearchText))
                {
                    this.Trailers = _originalTrailerList;
                }
                else
                {
                    this.Trailers = _originalTrailerList.Where(t => t.Trailer.Registration != null && t.Trailer.Registration.ToUpper().Contains(TrailerSearchText.ToUpper()));
                }

                this.TrailerSelectText = $"Select trailer for {this.VehicleRegistration} - Showing {Trailers.ToList().Count} of {_originalTrailerList.ToList().Count}";
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

        private MvxCommand<TrailerItemViewModel> _trailerSelectCommand;
        public ICommand TrailerSelectCommand
        {
            get
            {
                var title = "Confirm your trailer";
                return (_trailerSelectCommand = _trailerSelectCommand ?? new MvxCommand<TrailerItemViewModel>(async t => await ConfirmTrailerAsync(t.Trailer, title, t.Trailer.Registration)));
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

        private string _progressMessage;
        public string ProgressMessage {
            get
            {
                return _progressMessage;
            }
            set
            {
                _progressMessage = value;
                RaisePropertyChanged(() => ProgressMessage);
            }
        }

        private MvxCommand<Trailer> _notrailerSelectCommand;
        public ICommand NoTrailerSelectCommand
        {
            get
            {
                var title = "No trailer";
                var message = "Confirm you don't have a trailer";
                return (_notrailerSelectCommand = _notrailerSelectCommand ?? new MvxCommand<Trailer>(async t => await ConfirmTrailerAsync(null, title, message)));
            }
        }

        public string DefaultTrailerReg { get; private set; }

        #endregion

        #region Protected/Private Methods

        protected async Task SetDefaultTrailerRegAsync(string defaultTrailerReg)
        {
            this.DefaultTrailerReg = defaultTrailerReg;

            if (defaultTrailerReg != null)
                await this.GetTrailerModelsAsync();
        }

        protected abstract Task ConfirmTrailerAsync(Trailer trailer, string title, string message);

        protected async Task UpdateTrailerListAsync()
        {
            this.IsBusy = true;
            ProgressMessage = "Updating Trailers.";

            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
                return;
            }

            IDictionary<string, IEnumerable<Models.BaseVehicle>> vehicleViewVehicles;

            try
            {
                var vehicleViews = await _gatewayService.GetVehicleViewsAsync();

                vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehiclesAsync(vehicleView.Title));
                }
            }
            catch (TaskCanceledException)
            {
                // Although we have used reachability to determine that there is an available network connection,
                // it is still possible for the data fetch to fail which triggers a TaskCanceledException.
                _toast.Show("Connection failure!");
                return;
            }

            var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
            var trailers = vehiclesAndTrailers.Where(bv => bv.IsTrailer).Select(bv => new Models.Trailer(bv));

            if (trailers != null)
            {
                await _repositories.TrailerRepository.DeleteAllAsync();

                await _repositories.TrailerRepository.InsertAsync(trailers);

                await GetTrailerModelsAsync();

                //Recalls the filter text if there is text in the search field.
                if (TrailerSearchText != null)
                    FilterList();
            }
            this.IsBusy = false;
            await UpdateVehicleListAsync();
            await UpdateSafetyProfilesAsync();
            
        }

        protected async Task UpdateSafetyProfilesAsync()
        {
            ProgressMessage = "Updating Safety Check Profiles.";
            this.IsBusy = true;
            var safetyProfileRepository = _repositories.SafetyProfileRepository;

            // First check if we have a internet connection. If we do go and get the latest safety checks from Blue Sphere.
            if (_reachability.IsConnected())
            {
                IEnumerable<SafetyProfile> safetyProfiles = null;

                try
                {
                    safetyProfiles = await _gatewayService.GetSafetyProfilesAsync();
                }
                catch (TaskCanceledException)
                {
                    // Although we have used reachability to determine that there is an available network connection,
                    // it is still possible for the data fetch to fail which triggers a TaskCanceledException.
                }

                if (safetyProfiles != null)
                {
                    await safetyProfileRepository.DeleteAllAsync();
                    await safetyProfileRepository.InsertAsync(safetyProfiles);
                }
            }

            this.IsBusy = false;

            var profiles = await safetyProfileRepository.GetAllAsync();

            if (profiles.Count() == 0)
                await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("No Profiles Found.");
        }

        protected async Task UpdateVehicleListAsync()
        {
            ProgressMessage = "Updating Vehicles.";
            this.IsBusy = true;
            if (!_reachability.IsConnected())
            {
                _toast.Show("No internet connection!");
                return;
            }

            IDictionary<string, IEnumerable<Models.BaseVehicle>> vehicleViewVehicles;

            try
            {
                var vehicleViews = await _gatewayService.GetVehicleViewsAsync();

                vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehiclesAsync(vehicleView.Title));
                }
            }
            catch (TaskCanceledException)
            {
                // Although we have used reachability to determine that there is an available network connection,
                // it is still possible for the data fetch to fail which triggers a TaskCanceledException.
                _toast.Show("Connection failure!");
                return;
            }

            var vehiclesAndTrailers = vehicleViewVehicles.SelectMany(vvv => vvv.Value).DistinctBy(v => v.ID);
            var vehicles = vehiclesAndTrailers.Where(bv => !bv.IsTrailer).Select(bv => new Models.Vehicle(bv));

            if (vehicles != null && vehicles.Any())
            {
                await _repositories.VehicleRepository.DeleteAllAsync();
                await _repositories.VehicleRepository.InsertAsync(vehicles);
                // we need to updat the selected vehicle as the profile could have changed.
                var currentvehicle = vehicles.First(v => v.ID == _infoService.CurrentVehicle.ID);
                _infoService.CurrentVehicle = currentvehicle;
            }

            this.IsBusy = false;
        }

        private async Task GetTrailerModelsAsync()
        {
            var data = await _repositories.TrailerRepository.GetAllAsync();
            _originalTrailerList = data.Select(x => new TrailerItemViewModel() 
            { 
                Trailer = x,
                IsDefault = (string.IsNullOrEmpty(this.DefaultTrailerReg)) ? false : x.Registration == this.DefaultTrailerReg
            });

            this.FilterList();
        }

        #endregion

    }


    public class TrailerItemViewModel : MvxViewModel
    {

        private bool _isDefault;



        public Trailer Trailer { get; set; }
        public bool IsDefault 
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                RaisePropertyChanged(() => TrailerText);
            }
        }

        public string TrailerText
        {
            get
            {
                return (IsDefault) ? Trailer.Registration + " (as order)" : Trailer.Registration;
            }
        }


    }

}
