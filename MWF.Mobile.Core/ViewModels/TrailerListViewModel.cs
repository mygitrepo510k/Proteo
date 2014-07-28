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
        private readonly IToast _toast;
        private readonly IReachability _reachability;
        private readonly IStartupService _startupService;

        public TrailerListViewModel(IGatewayService gatewayService, IRepositories repositories, IReachability reachabibilty,
            IToast toast, IStartupService startupService)
        {
            _gatewayService = gatewayService;
            _toast = toast;
            _reachability = reachabibilty;
            _startupService = startupService;

            _repositories = repositories;
            Trailers = _originalTrailerList = _repositories.TrailerRepository.GetAll();

            _trailerListCount = Trailers.ToList().Count(); 

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
            get { return "Select trailer for " + VehicleRegistration + " - Showing "; }
        }

        public String VehicleRegistration
        {
            get
            {
                return _repositories.VehicleRepository.GetByID(_startupService.LoggedInDriver.LastVehicleID).Registration;
            }
        }

        private int _trailerListCount;
        public int TrailerListCount
        {
            get { return _trailerListCount; }
            set { _trailerListCount = value; RaisePropertyChanged(() => TrailerListCount); }
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
                var message = "Are you sure you don't want to select a trailer.";
                return (_notrailerSelectorCommand = _notrailerSelectorCommand ?? new MvxCommand<Trailer>(t => TrailerDetail(null, message)));
            }
        }

        public async void TrailerDetail(Trailer trailer, string message)
        {
            Guid trailerID = Guid.Empty;

            if (trailer != null)
            {
                trailerID = trailer.ID;
            }

            // Try and update safety profiles before continuing
            await UpdateSafetyProfilesAsync();

            //This will take to the next view model with a trailer value of null.
            Mvx.Resolve<IUserInteraction>().Confirm(message, isConfirmed =>
            {
                if (isConfirmed)
                {
                    _startupService.LoggedInDriver.LastSecondaryVehicleID = trailerID;
                    _startupService.CurrentTrailer = trailer;
                    ShowViewModel<SafetyCheckViewModel>();
                }
            }, "Confirm trailer", "Select");
        }

        //This is method associated with the search button in the action bar.
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; RaisePropertyChanged(() => SearchText); FilterList(); }
        }


        private void FilterList()
        {
            Trailers = _originalTrailerList.Where(t => t.Registration.ToUpper().Contains(SearchText.ToUpper()));
            TrailerListCount = Trailers.ToList().Count(); 
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
                    Mvx.Resolve<IUserInteraction>().Alert("No Profiles Found - Redirecting to Manifest Screen.");
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

                    TrailerListCount = Trailers.ToList().Count(); 

                    //Recalls the filter text if there is text in the search field.
                    if (SearchText != null)
                    {
                        FilterList();
                    }
                }
            }
        }
    }
}
