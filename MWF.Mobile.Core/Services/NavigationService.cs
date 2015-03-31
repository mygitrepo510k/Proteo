using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Presentation;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using Chance.MvvmCross.Plugins.UserInteraction;
using System.Threading.Tasks;


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
        private IMainService _mainService;
        private ICustomPresenter _presenter;
        IStartupService _startupService;
        IRepositories _repositories;
        ICloseApplication _closeApplication;
        private IDataChunkService _dataChunkService;


        #endregion

        #region Construction

        public NavigationService(
            ICustomPresenter presenter,
            IStartupService startupService,
            ICloseApplication closeApplication,
            IRepositories repositories,
            IGatewayPollingService gatewayPollingService,
            IMainService mainService,
            IDataChunkService dataChunkService)
        {
            _forwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<NavData>>();
            _backwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<NavData>>();
            _navDataDictionary = new Dictionary<Guid, Tuple<object, Dictionary<string, object>>>();
            _presenter = presenter;

            _mainService = mainService;
            _repositories = repositories;
            _gatewayPollingService = gatewayPollingService;
            _startupService = startupService;
            _closeApplication = closeApplication;
            _dataChunkService = dataChunkService;

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
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

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
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action<NavData> GetBackNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

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
            //TODO: Safety check on log out when profile dictates.

            if ((VehicleSafetyProfile != null && VehicleSafetyProfile.DisplayAtLogoff)
                || (TrailerSafetyProfile != null && TrailerSafetyProfile.DisplayAtLogoff))
            {
                Mvx.Resolve<IUserInteraction>().Alert("You would of done a safety check here! To be Implemented.", () =>
                {
                    _gatewayPollingService.StopPollingTimer();
                    MoveTo(typeof(StartupViewModel), navData);
                });
            }
            else
            {
                // Stop the gateway polling service before we "logout" the user.
                _gatewayPollingService.StopPollingTimer();
                MoveTo(typeof(StartupViewModel), navData);
            }
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
                return _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == _startupService.CurrentVehicle.SafetyCheckProfileIntLink).SingleOrDefault();
            }
        }

        private SafetyProfile TrailerSafetyProfile
        {
            get
            {
                return (_startupService.CurrentTrailer != null) ? _repositories.SafetyProfileRepository.GetAll().Where(spv => spv.IntLink == _startupService.CurrentTrailer.SafetyCheckProfileIntLink).SingleOrDefault() : null;
            }
        }

        private Enums.SafetyCheckStatus SafetyCheckStatus
        {
            get { return SafetyCheckData.GetOverallStatus(_startupService.GetCurrentSafetyCheckData().Select(scd => scd.GetOverallStatus())); }
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

            var isConfirmed = await Mvx.Resolve<IUserInteraction>().ConfirmAsync("Do you want to enter a comment for this instruction?", "", "Yes", "No");

            if (isConfirmed)
            {
                advanceToCommentScreen = true;
                this.ShowViewModel<InstructionCommentViewModel>(navData);
            }

            return advanceToCommentScreen;
        }

        /// <summary>
        /// Adds navigation data object to our dictionary so it can be retreived by a view model after a navigation action has completed
        /// MVVM cross only allows passing of simple objects via serialization. Navigation improves this by having MVVM Cross pass a GUID
        /// linked to the full objects that can be retrieved after the navigated to ViewModel is shown
        /// a 
        /// </summary>
        /// <param name="navData"></param>
        private void AddNavDataToDictionary(NavData navData)
        {
            navData.NavGUID = Guid.NewGuid();

            var tuple = Tuple.Create<object, Dictionary<string, object>>(navData.GetData(), navData.OtherData);
            _navDataDictionary.Add(navData.NavGUID, tuple);

        }

        /// <summary>
        /// Once the user confirms the prompt it updates the instruction to complete and then sends off the Complete chunk to Bluesphere.
        /// </summary>
        private void CompleteInstruction(NavData<MobileData> navData)
        {
            Mvx.Resolve<IUserInteraction>().Confirm("Do you wish to complete?", isConfirmed =>
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
            _dataChunkService.SendDataChunk(navData.GetDataChunk(), navData.Data, _mainService.CurrentDriver, _mainService.CurrentVehicle);
            this.ShowViewModel<ManifestViewModel>();
        }


        private void DriverLogIn()
        {
            _startupService.DriverLogIn();
            _gatewayPollingService.StartPollingTimer();
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

            //TODO: Implement back button
            InsertCustomNavAction<MainViewModel, InstructionTrailerViewModel>(InstructionTrailer_CustomAction);

            //TODO: Implement back button
            InsertCustomNavAction<MainViewModel, InstructionCommentViewModel>(InstructionComment_CustomAction);

            InsertCustomNavAction<MainViewModel, InstructionSignatureViewModel>(InstructionSignature_CustomAction);

            InsertCustomBackNavAction<MainViewModel, OrderViewModel>(Order_CustomBackAction);
            InsertNavAction<MainViewModel, OrderViewModel>(typeof(ReviseQuantityViewModel));

            InsertNavAction<MainViewModel, ReviseQuantityViewModel>(typeof(OrderViewModel));

            InsertCustomNavAction<MainViewModel, InstructionTrunkProceedViewModel>(InstructionTrunkProceed_CustomAction);

            InsertCustomNavAction<MainViewModel, MessageViewModel>(Message_CustomAction);
            InsertCustomBackNavAction<MainViewModel, MessageViewModel>(Message_CustomBackAction);

            // Side bar Activity
            InsertCustomNavAction<MainViewModel, CameraViewModel>(SidebarNavigation_CustomAction);
            InsertCustomBackNavAction<MainViewModel, CameraViewModel>(SidebarNavigation_CustomAction);

            InsertCustomNavAction<MainViewModel, DisplaySafetyCheckViewModel>(SidebarNavigation_CustomAction);
            InsertCustomBackNavAction<MainViewModel, DisplaySafetyCheckViewModel>(SidebarNavigation_CustomAction);

            InsertCustomNavAction<MainViewModel, SafetyCheckViewModel>(SafetyCheck_CustomAction);        //to odometer, signature screen or main activity (manifest)
            InsertNavAction<MainViewModel, OdometerViewModel>(typeof(SafetyCheckSignatureViewModel));
            InsertCustomNavAction<MainViewModel, SafetyCheckSignatureViewModel>(Signature_CustomAction_Login);

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
            if (navData is NavData<Item>)
            {
                ShowViewModel<OrderViewModel>(navData);
            }
            else if (navData is NavData<Models.Instruction.Trailer>)
            {
                ShowViewModel<InstructionTrailerViewModel>(navData);
            }
            else if (navData is NavData<MobileData>)
            {
                var mobileDataNav = (NavData<MobileData>)navData;
                _dataChunkService.SendDataChunk(mobileDataNav.GetDataChunk(), mobileDataNav.Data, _mainService.CurrentDriver, _mainService.CurrentVehicle);

                ShowViewModel<InstructionOnSiteViewModel>(mobileDataNav);
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
            if (navData is NavData<Item>)
            {
                ShowViewModel<OrderViewModel>(navData);
            }
            else if (navData is NavData<MobileData>)
            {

                var mobileNavData = navData as NavData<MobileData>;

                var additionalContent = mobileNavData.Data.Order.Additional;
                var itemAdditionalContent = mobileNavData.Data.Order.Items.First().Additional;

                // Debug Code
                //additionalContent.IsTrailerConfirmationEnabled = false;
                //additionalContent.CustomerSignatureRequiredForCollection = true;
                //additionalContent.CustomerSignatureRequiredForDelivery = true;
                //itemAdditionalContent.BypassCommentsScreen = true;

                //Collection
                if (mobileNavData.Data.Order.Type == Enums.InstructionType.Collect)
                {
                    if (additionalContent.IsTrailerConfirmationEnabled)
                    {
                        this.ShowViewModel<InstructionTrailerViewModel>(mobileNavData);
                        return;
                    }
                }

                if (!itemAdditionalContent.BypassCommentsScreen)
                {
                    bool hasAdvanced = await ConfirmCommentAccess(mobileNavData);
                    if (hasAdvanced) return;
                }

                if (((additionalContent.CustomerNameRequiredForDelivery || additionalContent.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
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
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;

                if (mobileNavData.Data.ProgressState == Enums.InstructionProgress.NotStarted)
                {
                    this.ShowViewModel<InstructionViewModel>(navData);
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
            }

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

                if (((additionalContent.CustomerNameRequiredForDelivery || additionalContent.CustomerSignatureRequiredForDelivery) && mobileNavData.Data.Order.Type == Enums.InstructionType.Deliver) ||
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
        public void InstructionTrunkProceed_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileNavData = navData as NavData<MobileData>;
                CompleteInstruction(mobileNavData);
            }
        }

        /// <summary>
        ///  Message screen, completes messages with and without points and goes back to manifest screen
        /// </summary>
        public void Message_CustomAction(NavData navData)
        {
            if (navData is NavData<MobileData>)
            {
                var mobileData = navData as NavData<MobileData>;
                SendMobileData(mobileData);
            }
        }

        #endregion Custom Mapping Actions

        #region CustomBackActions

        public void Instruction_CustomBackAction(NavData navData)
        {
            this.ShowViewModel<ManifestViewModel>();
        }

        public void Message_CustomBackAction(NavData navData)
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
