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

        public async override void Start()
        {
            base.Start();
            await GetTrailerModels();
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


        private string _vehicleRegistration = "";
        public String VehicleRegistration
        {
            get
            {
                return _vehicleRegistration;
            }
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
                Trailers = _originalTrailerList.Where(t => t.Trailer.Registration != null && t.Trailer.Registration.ToUpper().Contains(TrailerSearchText.ToUpper()));
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

        private string _defaultTrailerReg;
        public string DefaultTrailerReg
        {
            get
            {
                return _defaultTrailerReg;
            }
            set
            {
                _defaultTrailerReg = value;
                if (_defaultTrailerReg!=null)
                     GetTrailerModels();
            }
        }

        #endregion

        #region Protected/Private Methods

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
                var vehicleViews = await _gatewayService.GetVehicleViews();

                vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
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

                await GetTrailerModels();

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
                    safetyProfiles = await _gatewayService.GetSafetyProfiles();
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
                Mvx.Resolve<ICustomUserInteraction>().Alert("No Profiles Found.");
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
                var vehicleViews = await _gatewayService.GetVehicleViews();

                vehicleViewVehicles = new Dictionary<string, IEnumerable<Models.BaseVehicle>>(vehicleViews.Count());

                foreach (var vehicleView in vehicleViews)
                {
                    vehicleViewVehicles.Add(vehicleView.Title, await _gatewayService.GetVehicles(vehicleView.Title));
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

            if (vehicles != null)
            {
                await _repositories.VehicleRepository.DeleteAllAsync();
                await _repositories.VehicleRepository.InsertAsync(vehicles);
                // we need to updat the selected vehicle as the profile could have changed.
                var currentvehicle = vehicles.First(v => v.ID == _infoService.CurrentVehicle.ID);
                _infoService.CurrentVehicle = currentvehicle;
            }
            this.IsBusy = false;
           

        }

        private async Task GetTrailerModels()
        {

            var _vehicle = await _repositories.VehicleRepository.GetByIDAsync(_infoService.LoggedInDriver.LastVehicleID);
            _vehicleRegistration = _vehicle.Registration;

            var data = await _repositories.TrailerRepository.GetAllAsync();
            this.Trailers =  _originalTrailerList = data.Select(x => new TrailerItemViewModel() 
                                                                                                        { 
                                                                                                            Trailer = x,
                                                                                                            IsDefault = (string.IsNullOrEmpty(this.DefaultTrailerReg)) ? false : x.Registration == this.DefaultTrailerReg
                                                                                                        });
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
