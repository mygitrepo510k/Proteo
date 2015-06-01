using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Presentation;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Utilities;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;


namespace MWF.Mobile.Core.Services
{

    /// <summary>
    /// Class to centralise navigation logic. Stores a navigation graph as two dictionaries (forward and backward)
    /// that map a Activity/Fragment view model graph node to the action that will navigate to the next node.
    /// The actual mappings that define the MWF.Mobile navigation graph are in the SetMappings function.
    /// </summary>
    public class NavigationService : MvxNavigatingObject, INavigationService
    {

        #region Private Members

        private Dictionary<Tuple<Type, Type>, Action<NavData>> _forwardNavActionDictionary;
        private Dictionary<Tuple<Type, Type>, Action<NavData>> _backwardNavActionDictionary;
        private Dictionary<Guid, Tuple<Object, Dictionary<string, Object>>> _navDataDictionary;
        private NavData _currentNavData;


        private readonly IGatewayPollingService _gatewayPollingService;
        private readonly IInfoService _infoService;
        private readonly IDataChunkService _dataChunkService;
        private readonly ISafetyCheckService _safetyCheckService;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;
        private ICustomPresenter _presenter;
        private bool _inLogoutSafetyCheck = false;
        
        IRepositories _repositories;
        ICloseApplication _closeApplication;      
        private Timer _loginSessionTimer;
        private readonly IMvxMessenger _messenger = null;
        private MvxSubscriptionToken _notificationToken;

        #endregion

        #region Construction

        public NavigationService(
            ICustomPresenter presenter,
            IInfoService infoService,
            ICloseApplication closeApplication,
            IRepositories repositories,
            IGatewayPollingService gatewayPollingService,
            IDataChunkService dataChunkService,
            IMvxMessenger messenger,
            ISafetyCheckService safetyCheckService,
            IGatewayQueuedService gatewayQueuedService, 
            IGpsService gpsService)
        {
            _forwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<NavData>>();
            _backwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<NavData>>();
            _navDataDictionary = new Dictionary<Guid, Tuple<object, Dictionary<string, object>>>();
            _presenter = presenter;

            _repositories = repositories;
            _gatewayPollingService = gatewayPollingService;
            _infoService = infoService;
            _closeApplication = closeApplication;
            _dataChunkService = dataChunkService;
            _safetyCheckService = safetyCheckService;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
            _messenger = messenger;

            _notificationToken = Mvx.Resolve<IMvxMessenger>().Subscribe<Messages.InvalidLicenseNotificationMessage>(m =>
                this.InvalidLicenseLogout()
             );

            SetMappings();
        }

        #endregion

        #region Public Methods


        public void InsertNavAction<T1, T2>(Type destinationNodeType)
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {

            if (!IsDestinationTypeValid(destinationNodeType)) throw new ArgumentException("destinationNodeType must derive from MvxViewModel");

            var key = CreateKey<T1, T2>();
            _forwardNavActionDictionary.Add(key, (parameters) => MoveTo(destinationNodeType, parameters));
        }


        public void InsertCustomNavAction<T1, T2>(Action<NavData> action)
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            _forwardNavActionDictionary.Add(key, action);
        }


        public void InsertBackNavAction<T1, T2>(Type destinationNodeType)
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            if (!IsDestinationTypeValid(destinationNodeType)) throw new ArgumentException("destinationNodeType must derive from MvxViewModel");

            var key = CreateKey<T1, T2>();
            _backwardNavActionDictionary.Add(key, (noParam) => MoveBackTo(destinationNodeType));
        }


        public void InsertCustomBackNavAction<T1, T2>(Action<NavData> action)
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            _backwardNavActionDictionary.Add(key, action);
        }


        public bool NavActionExists<T1, T2>()
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            return _forwardNavActionDictionary.ContainsKey(key);
        }


        public bool BackNavActionExists<T1, T2>()
            where T1 : MvxViewModel
            where T2 : MvxViewModel
        {
            var key = CreateKey<T1, T2>();
            return _backwardNavActionDictionary.ContainsKey(key);
        }


        public bool BackNavActionExists(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityViewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return _backwardNavActionDictionary.ContainsKey(key);
        }


        public Action<NavData> GetNavAction<T1, T2>()
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action<NavData> GetNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityViewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action<NavData> GetBackNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityViewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_backwardNavActionDictionary, key);
        }

        #endregion Public Methods

        #region INavigationService

        public void MoveToNext()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<NavData> navAction = this.GetNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownNavigationMappingException(currentActivityType, currentFragmentType);
            _currentNavData = null;

            navAction.Invoke(null);

        }


        public bool ShowModalViewModel<TViewModel, TResult>(BaseFragmentViewModel viewModel, NavData navData, Action<TResult> onResult)
        where TViewModel : IModalViewModel<TResult>
        {
            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            AddNavDataToDictionary(navData);
            _currentNavData = navData;

            return viewModel.ShowModalViewModel<TViewModel, TResult>(navData, onResult);
        }


        public void MoveToNext(NavData navData)
        {

            if (navData == null)
            {
                MoveToNext();
                return;
            }

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<NavData> navAction = this.GetNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownNavigationMappingException(currentActivityType, currentFragmentType);

            AddNavDataToDictionary(navData);
            _currentNavData = navData;

            navAction.Invoke(navData);

        }


        public void GoBack()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<NavData> navAction = this.GetBackNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownBackNavigationMappingException(currentActivityType, currentFragmentType);
            _currentNavData = null;

            navAction.Invoke(null);

        }

        public void GoBack(NavData navData)
        {

            if (navData == null)
            {
                GoBack();
                return;
            }

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<NavData> navAction = this.GetBackNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownBackNavigationMappingException(currentActivityType, currentFragmentType);

            AddNavDataToDictionary(navData);
            _currentNavData = navData;

            navAction.Invoke(navData);

        }

        public bool IsBackActionDefined()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            return BackNavActionExists(currentActivityType, currentFragmentType);
        }

        public void GoToManifest()
        {
            this.ShowViewModel<ManifestViewModel>();
        }

        public void Logout_Action(NavData navData)
        {

            if ((VehicleSafetyProfile != null && VehicleSafetyProfile.DisplayAtLogoff)
                || (TrailerSafetyProfile != null && TrailerSafetyProfile.DisplayAtLogoff))
            {
                _inLogoutSafetyCheck = true;
                this.ShowViewModel<SafetyCheckViewModel>();
            }
            else
            {
                DoLogout(navData);
            }
        }

        private async void InvalidLicenseLogout()
        {
            await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Your license is no longer valid. Logging out.");

            _infoService.LoggedInDriver.IsLicensed = false;
            _repositories.DriverRepository.Update(_infoService.LoggedInDriver);

            DoLogout(this.CurrentNavData);

        }

        private void DoLogout(NavData navData)
        {
            // Stop the gateway polling service before we "logout" the user.
            _gatewayPollingService.StopPollingTimer();
            this.StopLoginSessionTimer();

            _infoService.CurrentVehicle = null;
            _infoService.CurrentTrailer = null;
            _infoService.LoggedInDriver = null;

            MoveTo(typeof(StartupViewModel), navData);
        }

        public void PopulateNavData(NavData navData)
        {
            var tuple = _navDataDictionary[navData.NavGUID];
            navData.SetData(tuple.Item1);
            navData.OtherData = tuple.Item2;

            //remove the item data from the dictionary now
            _navDataDictionary.Remove(navData.NavGUID);

        }

        public NavData CurrentNavData
        {
            get { return _currentNavData; }
        }


        #endregion INavigationService

        #region Private Properties


        private SafetyProfile VehicleSafetyProfile
        {
            get
            {
                return _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == _infoService.CurrentVehicle.SafetyCheckProfileIntLink).SingleOrDefault();
            }
        }

        private SafetyProfile TrailerSafetyProfile
        {

            get
            {
                return GetTrailerSafetyProfile(_infoService.CurrentTrailer);
            }

        }

        private SafetyProfile GetTrailerSafetyProfile(Models.Trailer trailer)
        {

            return (trailer != null) ? _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == trailer.SafetyCheckProfileIntLink).SingleOrDefault() : null;

        }

        private Enums.SafetyCheckStatus SafetyCheckStatus
        {
            get { return SafetyCheckData.GetOverallStatus(_safetyCheckService.GetCurrentSafetyCheckData().Select(scd => scd.GetOverallStatus())); }
        }

        #endregion Private Properties

        #region Private Methods

        private void MoveTo(Type type, NavData navData)
        {
            this.ShowViewModel(type, navData);
        }

        private void MoveBackTo(Type type)
        {
            ChangePresentation(new Presentation.CloseUpToViewPresentationHint(type));
        }

        private Tuple<Type, Type> CreateKey<T1, T2>()
            where T1 : MvxViewModel
            where T2 : MvxViewModel
        {
            return Tuple.Create<Type, Type>(typeof(T1), typeof(T2));
        }

        private Tuple<Type, Type> CreateKey(Type activityType, Type fragmentType)
        {
            return Tuple.Create<Type, Type>(activityType, fragmentType);
        }

        private Action<NavData> GetNavActionWithKey(Dictionary<Tuple<Type, Type>, Action<NavData>> dictionary, Tuple<Type, Type> key)
        {
            Action<NavData> action = null;
            dictionary.TryGetValue(key, out action);
            return action;
        }

        private bool AreSourceTypesValid(Type activityType, Type fragmentType)
        {
            return typeof(BaseActivityViewModel).IsAssignableFrom(activityType) && typeof(BaseFragmentViewModel).IsAssignableFrom(fragmentType);
        }

        private bool IsDestinationTypeValid(Type destType)
        {
            return typeof(MvxViewModel).IsAssignableFrom(destType);
        }

        private async Task<bool> ConfirmCommentAccess(NavData navData)
        {
            bool advanceToCommentScreen = false;

            var isConfirmed = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("Do you want to enter a comment for this instruction?", "", "Yes", "No");

            if (isConfirmed)
            {
                advanceToCommentScreen = true;
                this.ShowViewModel<InstructionCommentViewModel>(navData);
            }

            return advanceToCommentScreen;
        }

        private async Task<bool> IsCleanInstruction(NavData navData)
        {
            var isClean = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync("Is the delivery clean?", "", "Yes", "No");

            if (!isClean)
            {
                this.ShowViewModel<InstructionClausedViewModel>(navData);
            }

            return isClean;
        }

        /// <summary>
        /// Adds navigation data object to our dictionary so it can be retreived by a view model after a navigation action has completed
        /// MVVM cross only allows passing of simple objects via serialization. Navigation improves this by having MVVM Cross pass a GUID
        /// linked to the full objects that can be retrieved after the navigated to ViewModel is shown
        /// </summary>
        /// <param name="navData"></param>
        private void AddNavDataToDictionary(NavData navData)
        {

            var tuple = Tuple.Create<object, Dictionary<string, object>>(navData.GetData(), navData.OtherData);
            _navDataDictionary[navData.NavGUID] = tuple;
        }

        /// <summary>
        /// Once the user confirms the prompt it updates the instruction to complete and then sends off the Complete chunk to Bluesphere.
        /// </summary>
        private void CompleteInstruction(NavData<MobileData> navData)
        {
            Mvx.Resolve<ICustomUserInteraction>().Confirm("Do you wish to complete?", isConfirmed =>
            {
                if (isConfirmed)
                    SendMobileData(navData);

            }, "Complete Instruction", "Confirm", "Cancel");
        }

        /// <summary>
        /// Updates the instruction/Message to complete and then sends off the Complete chunk to Bluesphere.
        /// </summary>
        private void SendMobileData(NavData<MobileData> navData)
        {
            navData.Data.ProgressState = Enums.InstructionProgress.Complete;
            _dataChunkService.SendDataChunk(navData.GetDataChunk(), navData.Data, _infoService.LoggedInDriver, _infoService.CurrentVehicle);

            // send any datachunks for addiotional instructions
            var additionalInstructions = navData.GetAdditionalInstructions();
            foreach (var additionalInstruction in additionalInstructions)
            {

                additionalInstruction.ProgressState = Enums.InstructionProgress.Complete;
                _dataChunkService.SendDataChunk(navData.GetAdditionalDataChunk(additionalInstruction), additionalInstruction, _infoService.LoggedInDriver, _infoService.CurrentVehicle);

            }


            this.ShowViewModel<ManifestViewModel>();
        }

        private void DriverLogIn()
        {
            DriverActivity currentDriver = new DriverActivity(_infoService.LoggedInDriver, _infoService.CurrentVehicle, Enums.DriverActivity.LogOn);
            currentDriver.Smp = _gpsService.GetSmpData(Enums.ReportReason.DriverLogOn);

            _gatewayQueuedService.AddToQueue("fwSetDriverActivity", currentDriver);

            this.StartLoginSessionTimer();
            _gatewayPollingService.StartPollingTimer();
        }

        private void StartLoginSessionTimer()
        {
            if (_loginSessionTimer == null)
            {
                var config = _repositories.ConfigRepository.Get();
                var sessionTimeoutInSeconds = config.SessionTimeoutInSeconds;

                if (sessionTimeoutInSeconds > 0)
                    _loginSessionTimer = new Timer(
                        async state =>
                        {
                            await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("Your login session has expired.");
                            this.DoLogout(null);
                        },
                        null,
                        sessionTimeoutInSeconds * 1000);
            }
            else
                _loginSessionTimer.Reset();
        }

        private void StopLoginSessionTimer()
        {
            if (_loginSessionTimer == null)
                return;

            _loginSessionTimer.Dispose();
            _loginSessionTimer = null;
        }

        #endregion Private Methods

        #region Mappings Definitions

        private void SetMappings()
        {
            // StartUp Activity
            InsertNavAction<StartupViewModel, CustomerCodeViewModel>(typeof(PasscodeViewModel));
            InsertNavAction<StartupViewModel, PasscodeViewModel>(typeof(VehicleListViewModel));
            InsertNavAction<StartupViewModel, VehicleListViewModel>(typeof(TrailerListViewModel));
            InsertNavAction<StartupViewModel, TrailerListViewModel>(typeof(SafetyCheckViewModel));
            InsertCustomNavAction<StartupViewModel, SafetyCheckViewModel>(SafetyCheck_CustomAction);        //to odometer, signature screen or main activity (manifest)      

            InsertNavAction<StartupViewModel, OdometerViewModel>(typeof(SafetyCheckSignatureViewModel));
            InsertCustomNavAction<StartupViewModel, SafetyCheckSignatureViewModel>(Signature_CustomAction_Login); //to either main activity (manifest) or back to driver passcode

            InsertCustomBackNavAction<StartupViewModel, PasscodeViewModel>(CloseApplication);               //Back from passcode closes app
            

            // Main Activity
            InsertCustomBackNavAction<MainViewModel, ManifestViewModel>(Logout_Action); // Back from manifest sends back to startup activity
            InsertCustomNavAction<MainViewModel, ManifestViewModel>(Manifest_CustomAction);

            InsertCustomBackNavAction<MainViewModel, InstructionViewModel>(Instruction_CustomBackAction);
            InsertCustomNavAction<MainViewModel, InstructionViewModel>(Instruction_CustomAction);

            InsertCustomBackNavAction<MainViewModel, InstructionOnSiteViewModel>(InstructionOnSite_CustomBackAction);
            InsertCustomNavAction<MainViewModel, InstructionOnSiteViewModel>(InstructionOnSite_CustomAction);

            // trailer select/safety check/signature for instruction
            InsertCustomNavAction<MainViewModel, InstructionTrailerViewModel>(InstructionTrailer_CustomAction);
            InsertCustomNavAction<MainViewModel, InstructionSafetyCheckViewModel>(TrailerSafetyCheck_CustomAction);
            InsertCustomNavAction<MainViewModel, InstructionSafetyCheckSignatureViewModel>(InstructionSafetyCheckSignature_CustomAction);

            InsertCustomNavAction<MainViewModel, BarcodeScanningViewModel>(Barcode_CustomAction);

            InsertCustomNavAction<MainViewModel, InstructionCommentViewModel>(InstructionComment_CustomAction);

            InsertCustomNavAction<MainViewModel, InstructionClausedViewModel>(InstructionClaused_CustomAction);

            InsertCustomNavAction<MainViewModel, InstructionSignatureViewModel>(InstructionSignature_CustomAction);

            InsertCustomBackNavAction<MainViewModel, OrderViewModel>(Order_CustomBackAction);
            InsertNavAction<MainViewModel, OrderViewModel>(typeof(ReviseQuantityViewModel));

            InsertNavAction<MainViewModel, ReviseQuantityViewModel>(typeof(OrderViewModel));

            InsertCustomNavAction<MainViewModel, InstructionTrunkProceedViewModel>(InstructionTrunkProceed_CustomAction);

            InsertCustomNavAction<MainViewModel, InboxViewModel>(Inbox_CustomAction);
            InsertCustomBackNavAction<MainViewModel, SafetyCheckViewModel>(SafetyCheck_CustomBackAction);


            // safety check on logout sequence
            InsertCustomNavAction<MainViewModel, SafetyCheckViewModel>(SafetyCheck_CustomAction);
            InsertNavAction<MainViewModel, OdometerViewModel>(typeof(SafetyCheckSignatureViewModel));
            InsertCustomNavAction<MainViewModel, SafetyCheckSignatureViewModel>(Signature_CustomAction_SidebarLogout);

            // Side bar Activity
            InsertCustomNavAction<MainViewModel, SidebarCameraViewModel>(SidebarNavigation_CustomAction);
            InsertCustomBackNavAction<MainViewModel, SidebarCameraViewModel>(SidebarNavigation_CustomAction);

            InsertCustomNavAction<MainViewModel, DisplaySafetyCheckViewModel>(SidebarNavigation_CustomAction);
            InsertCustomBackNavAction<MainViewModel, DisplaySafetyCheckViewModel>(SidebarNavigation_CustomAction);



        }



        #endregion Mappings Definitions

        #region Custom Mapping Actions

        private void CloseApplication(Object parameters)
        {
            _closeApplication.CloseApp();
        }

        /// <summary>
        /// Safety Check screen goes to main activity (manifest) if there are no profiles
        /// or odometer screen if odometer reading is required, safety check signature screen otherwise
        /// </summary>
        public void SafetyCheck_CustomAction(NavData navData)
        {

            if (VehicleSafetyProfile == null && TrailerSafetyProfile == null)
            {
                if (_presenter.CurrentActivityViewModel.GetType().Equals(typeof(ViewModels.StartupViewModel)))
                    this.DriverLogIn();

                MoveTo(typeof(MainViewModel), navData);
            }
            else
            {
                if (VehicleSafetyProfile != null && VehicleSafetyProfile.OdometerRequired)
                    this.ShowViewModel<OdometerViewModel>();
                else
                    this.ShowViewModel<SafetyCheckSignatureViewModel>();
            }

        }

        /// <summary>
        /// Signature screen goes back to driver pass code screen if we have any safety check failures
        /// and to main acticity (manifest) otherwise
        /// </summary>
        public void Signature_CustomAction_Login(NavData navData)
        {

            // commit safety check data to repositories and bluesphere
            _safetyCheckService.CommitSafetyCheckData();

            if (SafetyCheckStatus == Enums.SafetyCheckStatus.Failed)
            {
                MoveTo(typeof(StartupViewModel), navData);
            }
            else
            {
                if (_presenter.CurrentActivityViewModel.GetType().Equals(typeof(ViewModels.StartupViewModel)))
                    this.DriverLogIn();

                MoveTo(typeof(MainViewModel), navData);
            }

        }

        /// <summary>
        /// Commits the safety check and does the logout if we were doing a logout sfaety check
        ///  Otherwise this must have have been a safety check from the sidebar so go back to manifestscreen
        /// </summary>
        public void Signature_CustomAction_SidebarLogout(NavData navData)
        {
            if (_inLogoutSafetyCheck)
            {

                // commit safety check data to repositories and bluesphere
                _safetyCheckService.CommitSafetyCheckData();
                DoLogout(navData);
                _inLogoutSafetyCheck = false;
            }
            else
            {
                this.ShowViewModel<ManifestViewModel>();
            }

            

        }

        /// <summary>
        /// Manifest screen goes back to to instruction screen if we get a mobile data nav item
        /// and to main acticity (manifest) otherwise
        /// </summary>
        public void Manifest_CustomAction(NavData navData)
        {

            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;

                if (mobileNavData.Data.Order.Type == Enums.InstructionType.Collect || mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver)
                    this.ShowViewModel<InstructionViewModel>(mobileNavData);
                else if (mobileNavData.Data.Order.Type == Enums.InstructionType.TrunkTo || mobileNavData.Data.Order.Type == Enums.InstructionType.ProceedFrom)
                    this.ShowViewModel<InstructionTrunkProceedViewModel>(mobileNavData);
                else if (mobileNavData.Data.Order.Type == Enums.InstructionType.OrderMessage)
                    this.ShowViewModel<MessageViewModel>(mobileNavData);


            }

        }

        /// <summary>
        /// Instruction screen. If we're getting an "item" (order) then show the order in question
        /// If we're getting a trailer then show the select trailer screen
        /// If we're getting a mobile data then move on to InstructionOnSiteViewModel
        /// </summary>
        /// <param name="parameters"></param>
        public void Instruction_CustomAction(NavData navData)
        {

            if (navData is NavData<Models.Instruction.Trailer>)
            {

            }
            else if (navData is NavData<MobileData>)
            {
                if (navData.OtherData.IsDefined("IsTrailerEditFromInstructionScreen"))
                {
                    ShowViewModel<InstructionTrailerViewModel>(navData);
                }
                else
                {
                    var mobileDataNav = (NavData<MobileData>)navData;
                    // send "onsite" data chunk 
                    _dataChunkService.SendDataChunk(mobileDataNav.GetDataChunk(), mobileDataNav.Data, _infoService.LoggedInDriver, _infoService.CurrentVehicle);

                    ShowViewModel<InstructionOnSiteViewModel>(mobileDataNav);
                }
            }
        }

        /// <summary>
        /// Instruction on site screen, if the trailer selection is enabled then it will redirect to the trailer selection screen
        /// else if trailer selection is not enabled and the bypass comment screen is not then enabled then will it redirect to comment screen.
        /// else if trailer selection is not enabled and the bypass comment screen is enabled 
        /// and if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public async void InstructionOnSite_CustomAction(NavData navData)
        {
        if (navData is NavData<MobileData>)
            {

                var mobileNavData = navData as NavData<MobileData>;

                var additionalContent = mobileNavData.Data.Order.Additional;
                var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;
                var deliveryOptions = mobileNavData.GetWorseCaseDeliveryOptions();

                //Collection
                if (mobileNavData.Data.Order.Type == Enums.InstructionType.Collect)
                {

                    if (additionalContent.IsTrailerConfirmationEnabled || !Models.Trailer.SameAs(_infoService.CurrentTrailer, mobileNavData.Data.Order.Additional.Trailer))
                    {
                        // Note if trailer confirmation is not explicitly enabled, still need to cater for ambiguous case
                        // where the current trailer doesn't match the one specified on the order. Which one does the driver
                        // actually have attached and intend to use for the order?
                        var instructionTrailer = mobileNavData.Data.Order.Additional.Trailer;
                        string orderTrailerMessage = (instructionTrailer == null || instructionTrailer.TrailerId == null) ? "No trailer specified on instruction." : string.Format("Trailer specified on instruction is {0}.", instructionTrailer.TrailerId);
                        string currentTrailerMessage = (_infoService.CurrentTrailer == null) ? " You currently have no trailer." : string.Format(" Current trailer is {0}.", _infoService.CurrentTrailer.Registration);

                        string message = orderTrailerMessage + currentTrailerMessage;
                        var isConfirmed = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(message, "Change Trailer?", "Select Trailer", "Use Current");

                        if (isConfirmed)
                        {
                            this.ShowViewModel<InstructionTrailerViewModel>(mobileNavData);
                            return;
                        }
                        else
                        {
                            UpdateTrailerForInstruction(mobileNavData, _infoService.CurrentTrailer);
                        }
                    }
                }

                if (mobileNavData.Data.Order.IsBarcodeScanRequired())
                {
                    if (mobileNavData.Data.Order.HasBarcodes())
                    {
                        this.ShowViewModel<BarcodeScanningViewModel>(mobileNavData);
                        return;
                    }

                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("There are no barcodes to be scanned on this instruction.");
                }

                // Delivery Clean/Clause Prompt
                if (mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver &&
                    !deliveryOptions.BypassCleanClausedScreen)
                {
                    bool isClean = await IsCleanInstruction(mobileNavData);

                    if (!isClean) return;
                }

                if ((mobileNavData.Data.Order.Type == Enums.InstructionType.Collect && !itemAdditionalContent.BypassCommentsScreen) ||
                     (mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver &&  !deliveryOptions.BypassCommentsScreen))
                {
                    bool hasAdvanced = await ConfirmCommentAccess(mobileNavData);
                    if (hasAdvanced) return;
                }

                if (((deliveryOptions.CustomerNameRequiredForDelivery || deliveryOptions.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
                    ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && mobileNavData.Data.Order.Type == Enums.InstructionType.Collect))
                {
                    this.ShowViewModel<InstructionSignatureViewModel>(mobileNavData);
                    return;
                }

                CompleteInstruction(mobileNavData);

            }
        }


        /// <summary>
        /// Instruction trailer screen, if the bypass comment screen is not then enabled then will it redirect to comment screen.
        /// else if the bypass comment screen is enabled and if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public async void InstructionTrailer_CustomAction(NavData navData)
        {

            var mobileNavData = navData as NavData<MobileData>;
            Models.Trailer trailer = mobileNavData.OtherData["UpdatedTrailer"] as Models.Trailer;


            // Trailer differs from the current trailer
            if (trailer != null && (!Models.Trailer.SameAs(trailer, _infoService.CurrentTrailer)))
            {
                this.ShowViewModel<InstructionSafetyCheckViewModel>(mobileNavData);
            }
            else
            {
                UpdateTrailerForInstruction(mobileNavData, trailer);
                mobileNavData.OtherData["UpdatedTrailer"] = null;

                if (navData.OtherData.IsDefined("IsTrailerEditFromInstructionScreen"))
                {
                    mobileNavData.OtherData["IsTrailerEditFromInstructionScreen"] = null;
                    // Go back to the instruction screen
                    this.ShowViewModel<InstructionViewModel>(mobileNavData);
                }
                else
                {
                    // else trailer select was via collection on-site flow
                    await CompleteInstructionTrailerSelection(mobileNavData);
                    return;
                }
            }
        }


        private async Task CompleteInstructionTrailerSelection(NavData<MobileData> mobileNavData)
        {
            if (mobileNavData.OtherData.IsDefined("IsProceedFrom"))
            {
                mobileNavData.OtherData["IsProceedFrom"] = null;
                CompleteInstruction(mobileNavData);
                return;
            }

            var additionalContent = mobileNavData.Data.Order.Additional;
            var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;

          

            if (!itemAdditionalContent.BypassCommentsScreen)
            {
                bool hasAdvanced = await ConfirmCommentAccess(mobileNavData);
                if (hasAdvanced) return;
            }

            if (additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection)
            {
                this.ShowViewModel<InstructionSignatureViewModel>(mobileNavData);
                return;
            }

            CompleteInstruction(mobileNavData);
            return;
        }


        private void UpdateTrailerForInstruction(NavData<MobileData> mobileNavData, Models.Trailer trailer)
        {
            // Trailer differs from one on the order
            if (!Models.Trailer.SameAs(trailer, mobileNavData.Data.Order.Additional.Trailer))
            {
                // set the trailer back on the order
                if (trailer == null)
                    mobileNavData.Data.Order.Additional.Trailer = null;
                else
                {
                    if (mobileNavData.Data.Order.Additional.Trailer == null)
                        mobileNavData.Data.Order.Additional.Trailer = new Models.Instruction.Trailer();

                    mobileNavData.Data.Order.Additional.Trailer.TrailerId = trailer.Registration;
                }

                // send the revised trailer data chunk
                _dataChunkService.SendDataChunk(mobileNavData.GetDataChunk(), mobileNavData.Data, _infoService.LoggedInDriver, _infoService.CurrentVehicle, updateTrailer: true);
            }

            _infoService.CurrentTrailer = trailer;

            if (trailer != null)
                _infoService.LoggedInDriver.LastSecondaryVehicleID = trailer.ID;
        }


        public async void TrailerSafetyCheck_CustomAction(NavData navData)
        {

            var mobileNavData = navData as NavData<MobileData>;
            Models.Trailer trailer = mobileNavData.OtherData["UpdatedTrailer"] as Models.Trailer;


            if (GetTrailerSafetyProfile(trailer) == null)
            {
                //No trailer safety profile
                UpdateTrailerForInstruction(mobileNavData, trailer);

                // Trailer select was via the "Change Trailer" button
                if (navData.OtherData.IsDefined("IsTrailerEditFromInstructionScreen"))
                {
                    navData.OtherData["IsTrailerEditFromInstructionScreen"] = null;
                    this.ShowViewModel<InstructionViewModel>(mobileNavData);
                    return;
                }
                else
                {

                    await CompleteInstructionTrailerSelection(mobileNavData);
                    return;
                }
            }
            else
            {
                this.ShowViewModel<InstructionSafetyCheckSignatureViewModel>(navData);
            }

        }

        private async void Barcode_CustomAction(NavData navData)
        {
            var mobileNavData = navData as NavData<MobileData>;

            var additionalContent = mobileNavData.Data.Order.Additional;
            var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;
            var deliveryOptions = mobileNavData.GetWorseCaseDeliveryOptions();

            // Delivery Clean/Clause Prompt
            if (mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver &&
                !deliveryOptions.BypassCleanClausedScreen)
            {
                bool isClean = await IsCleanInstruction(mobileNavData);

                if (!isClean) return;
            }

            if ((mobileNavData.Data.Order.Type == Enums.InstructionType.Collect && !itemAdditionalContent.BypassCommentsScreen) ||
                    (mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver && !deliveryOptions.BypassCommentsScreen))
            {
                bool hasAdvanced = await ConfirmCommentAccess(mobileNavData);
                if (hasAdvanced) return;
            }

            if (((deliveryOptions.CustomerNameRequiredForDelivery || deliveryOptions.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
                               ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && mobileNavData.Data.Order.Type == Enums.InstructionType.Collect))
            {
                this.ShowViewModel<InstructionSignatureViewModel>(mobileNavData);
                return;
            }

            CompleteInstruction(mobileNavData);
        }


        /// <summary>
        /// Safety check screen when selecting a new trailer for an instruction. Either moves the use back to
        /// the instruction screen or moves then onto the rest of the collection on site flow
        /// 
        /// </summary>
        public async void InstructionSafetyCheckSignature_CustomAction(NavData navData)
        {

            var mobileNavData = navData as NavData<MobileData>;
            Models.Trailer trailer = mobileNavData.OtherData["UpdatedTrailer"] as Models.Trailer;

            UpdateTrailerForInstruction(mobileNavData, trailer);

            _safetyCheckService.CurrentTrailerSafetyCheckData = navData.OtherData["UpdatedTrailerSafetyCheckData"] as SafetyCheckData;

            // commit safety check data to repositories and bluesphere
            _safetyCheckService.CommitSafetyCheckData(trailerOnly: true);

            // clear all nav item data related to trailer selection flow
            mobileNavData.OtherData["UpdatedTrailer"] = null;
            mobileNavData.OtherData["UpdatedTrailerSafetyCheckData"] = null;

            // Failed safety checks Trailer select was via the "Change Trailer" button
            if (SafetyCheckStatus == Enums.SafetyCheckStatus.Failed || navData.OtherData.IsDefined("IsTrailerEditFromInstructionScreen"))
            {
                if (SafetyCheckStatus == Enums.SafetyCheckStatus.Failed)
                    await Mvx.Resolve<ICustomUserInteraction>().AlertAsync("As a result of the safety check failure you will be returned to the instruction screen.", "Failed Safety Check");

                mobileNavData.OtherData["IsTrailerEditFromInstructionScreen"] = null;
                this.ShowViewModel<InstructionViewModel>(mobileNavData);
                return;
            }
            else
            {
                // otherwise continue with collection on site flow
                await CompleteInstructionTrailerSelection(mobileNavData);
                return;
            }
        }

        public async void InstructionClaused_CustomAction(NavData navData)
        {
            var mobileNavData = navData as NavData<MobileData>;

            var additionalContent = mobileNavData.Data.Order.Additional;
            var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;
            var deliveryOptions = mobileNavData.GetWorseCaseDeliveryOptions();

            if ((mobileNavData.Data.Order.Type == Enums.InstructionType.Collect && !itemAdditionalContent.BypassCommentsScreen) ||
                 (mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver && !deliveryOptions.BypassCommentsScreen))
            {
                bool hasAdvanced = await ConfirmCommentAccess(mobileNavData);
                if (hasAdvanced) return;
            }

            if (((deliveryOptions.CustomerNameRequiredForDelivery || deliveryOptions.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
                    ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && mobileNavData.Data.Order.Type == Enums.InstructionType.Collect))
            {
                this.ShowViewModel<InstructionSignatureViewModel>(mobileNavData);
                return;
            }

            CompleteInstruction(mobileNavData);
        }

        /// <summary>
        /// Instruction comment screen, if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public void InstructionComment_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;

                var additionalContent = mobileNavData.Data.Order.Additional;
                var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;
                var deliveryOptions = mobileNavData.GetWorseCaseDeliveryOptions();

                if (((deliveryOptions.CustomerNameRequiredForDelivery || deliveryOptions.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
                   ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && mobileNavData.Data.Order.Type == Enums.InstructionType.Collect))
                {
                    this.ShowViewModel<InstructionSignatureViewModel>(mobileNavData);
                    return;
                }

                CompleteInstruction(mobileNavData);


            }
        }

        /// <summary>
        /// Instruction signature screen, complete instruction and go back to manifest screen
        /// </summary>
        public void InstructionSignature_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;
                CompleteInstruction(mobileNavData);
            }
        }

        /// <summary>
        /// Instruction TrunkTo/Proceed screens, completes the instruction and goes back to manifest screen
        /// </summary>
        public async void InstructionTrunkProceed_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;
                var additionalContent = mobileNavData.Data.Order.Additional;

                if (additionalContent.IsTrailerConfirmationEnabled && mobileNavData.Data.Order.Type == Enums.InstructionType.ProceedFrom)
                {
                    var instructionTrailer = mobileNavData.Data.Order.Additional.Trailer;
                    string orderTrailerMessage = (instructionTrailer == null || instructionTrailer.TrailerId == null) ? "No trailer specified on instruction." : string.Format("Trailer specified on instruction is {0}.", instructionTrailer.TrailerId);
                    string currentTrailerMessage = (_infoService.CurrentTrailer == null) ? " You currently have no trailer." : string.Format(" Current trailer is {0}.", _infoService.CurrentTrailer.Registration);

                    string message = orderTrailerMessage + currentTrailerMessage;
                    var isConfirmed = await Mvx.Resolve<ICustomUserInteraction>().ConfirmAsync(message, "Change Trailer?", "Select Trailer", "Use Current");

                    if (isConfirmed)
                    {
                        mobileNavData.OtherData["IsProceedFrom"] = true;
                        this.ShowViewModel<InstructionTrailerViewModel>(mobileNavData);
                        return;
                    }
                    else
                    {
                        UpdateTrailerForInstruction(mobileNavData, _infoService.CurrentTrailer);
                    }
                }

                CompleteInstruction(mobileNavData);
            }
        }

        private void Inbox_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;

                this.ShowViewModel<MessageViewModel>(mobileNavData);
            }
        }



        #endregion Custom Mapping Actions

        #region CustomBackActions


        public void SafetyCheck_CustomBackAction(NavData navData)
        {
            _inLogoutSafetyCheck = false;
            this.ShowViewModel<ManifestViewModel>();
        }


        public void Instruction_CustomBackAction(NavData navData)
        {
            this.ShowViewModel<ManifestViewModel>();
        }


        /// <summary>
        /// Order screen depending on the state of the instruction then it will go to the instruction on site screen if its
        /// Progress: Onsite, else it will go to the instruction screen the other times.
        /// </summary>
        public void Order_CustomBackAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var navMobileData = navData as NavData<MobileData>;

                switch (navMobileData.Data.ProgressState)
                {
                    case MWF.Mobile.Core.Enums.InstructionProgress.NotStarted:
                        this.ShowViewModel<InstructionViewModel>(navMobileData);
                        break;
                    case MWF.Mobile.Core.Enums.InstructionProgress.Driving:
                        this.ShowViewModel<InstructionViewModel>(navMobileData);
                        break;
                    case MWF.Mobile.Core.Enums.InstructionProgress.OnSite:
                        this.ShowViewModel<InstructionOnSiteViewModel>(navMobileData);
                        break;
                }
            }
        }

        public void InstructionOnSite_CustomBackAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var navMobileData = navData as NavData<MobileData>;
                this.ShowViewModel<InstructionViewModel>(navMobileData);
            }
        }

        public void SidebarNavigation_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var navMobileData = navData as NavData<MobileData>;

                if (navMobileData.Data == null)
                {
                    this.ShowViewModel<ManifestViewModel>();
                }
                else
                {

                    switch (navMobileData.Data.ProgressState)
                    {
                        case MWF.Mobile.Core.Enums.InstructionProgress.NotStarted:
                            this.ShowViewModel<InstructionViewModel>(navMobileData);
                            break;
                        case MWF.Mobile.Core.Enums.InstructionProgress.Driving:
                            this.ShowViewModel<InstructionViewModel>(navMobileData);
                            break;
                        case MWF.Mobile.Core.Enums.InstructionProgress.OnSite:
                            this.ShowViewModel<InstructionOnSiteViewModel>(navMobileData);
                            break;
                    }
                }
            }
            else
            {
                this.ShowViewModel<ManifestViewModel>();
            }
        }

        #endregion CustomBackActions

    }

    #region Exception Classes

    public class UnknownNavigationMappingException : Exception
    {
        public UnknownNavigationMappingException(Type activityType, Type fragmentType)
            : base(string.Format("No mapping defined for {0} activity / {1} fragment", activityType, fragmentType))
        {
        }
    }

    public class UnknownBackNavigationMappingException : Exception
    {
        public UnknownBackNavigationMappingException(Type activityType, Type fragmentType)
            : base(string.Format("No backward mapping defined for {0} activity / {1} fragment", activityType, fragmentType))
        {
        }
    }

    #endregion

}
