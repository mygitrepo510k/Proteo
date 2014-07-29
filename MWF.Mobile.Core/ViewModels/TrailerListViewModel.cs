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

    public class TrailerListViewModel
        : BaseFragmentViewModel
    {

        private Services.IGatewayService _gatewayService;
        private IEnumerable<Trailer> _originalTrailerList;

        private readonly IRepositories _repositories;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IToast _toast;
        private readonly IReachability _reachability;
        private readonly IStartupService _startupService;

        public TrailerListViewModel(IVehicleRepository vehicleRepository, IGatewayService gatewayService, IRepositories repositories, IReachability reachabibilty,
            IToast toast, IStartupService startupService)
        {
            _gatewayService = gatewayService;
            _toast = toast;
            _reachability = reachabibilty;
            _startupService = startupService;

            _repositories = repositories;
            Trailers = _originalTrailerList = _repositories.TrailerRepository.GetAll();
            _vehicleRepository = vehicleRepository;

            _trailersListCount = FilteredtrailerCount;
        }

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
            get { return "Select trailer for " + VehicleRegistration + " - Showing " + FilteredtrailerCount + " of " + TrailerListCount; }
        }

        private int _trailersListCount;
        public int TrailerListCount
        {
            get { return _trailersListCount; }
            set { _trailersListCount = value; }
        }

        public int FilteredtrailerCount
        {
            get { return Trailers.ToList().Count; }
        }

        public String VehicleRegistration
        {
            get
            {
                return _repositories.VehicleRepository.GetByID(_startupService.LoggedInDriver.LastVehicleID).Registration;
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
            get { return "Please wait while we setup your safety checks..."; }
        }
  

        private IEnumerable<Trailer> _trailers;
        public IEnumerable<Trailer> Trailers
        {
            get { return _trailers; }
            set { _trailers = value; RaisePropertyChanged(() => Trailers); }
        }

        private MvxCommand<Trailer> _trailerSelectorCommand;
        public ICommand TrailerSelectorCommand
        {
            get
            {
                return (_trailerSelectorCommand = _trailerSelectorCommand ?? new MvxCommand<Trailer>(t => TrailerDetail(t, t.Registration)));
            }
        }

        private MvxCommand<Trailer> _notrailerSelectorCommand;
        public ICommand NoTrailerSelectorCommand
        {
            get
            {
                var message = "Confirm you don't have a trailer";
                return (_notrailerSelectorCommand = _notrailerSelectorCommand ?? new MvxCommand<Trailer>(t => TrailerDetail(null, message)));
            }
        }

        public async void TrailerDetail(Trailer trailer, string message)
        {

            this.IsBusy = true;
            Guid trailerID = Guid.Empty;

            if (trailer != null)
            {
                trailerID = trailer.ID;
            }

            await UpdateVehicleListAsync();
 
            await UpdateTrailerListAsync();
            // Try and update safety profiles before continuing
            await UpdateSafetyProfilesAsync();

            this.IsBusy = false;
            //This will take to the next view model with a trailer value of null.
            Mvx.Resolve<IUserInteraction>().Confirm(message, isConfirmed =>
            {
                if (isConfirmed)
                {
                    _startupService.LoggedInDriver.LastSecondaryVehicleID = trailerID;
                    _startupService.CurrentTrailer = trailer;
                    ShowViewModel<SafetyCheckViewModel>();
                    
                }
            }, "Confirm your trailer", "Confirm");
        }

        //This is method associated with the search button in the action bar.
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); FilterList(); RaisePropertyChanged(() => TrailerSelectText); }
        }


        private void FilterList()
        {
            Trailers = _originalTrailerList.Where(t => t.Registration.ToUpper().Contains(SearchText.ToUpper()));
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

        public async Task UpdateSafetyProfilesAsync()
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
                if(safetyProfileRepository.GetAll().ToList().Count == 0)
                {
                    Mvx.Resolve<IUserInteraction>().Alert("No Profiles Found.");
                }
            }
        }

        public async Task UpdateTrailerListAsync()
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
                    if (SearchText != null)
                    {
                        FilterList();
                    }
                }
            }
        }

        public async Task UpdateVehicleListAsync()
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
                    _vehicleRepository.DeleteAll();

                    _vehicleRepository.Insert(vehicles);

                   
                }
            }
        }
    }
}
