﻿using System;
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
using Chance.MvvmCross.Plugins.UserInteraction;


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

        private Dictionary<Tuple<Type, Type>, Action<Object>> _forwardNavActionDictionary;
        private Dictionary<Tuple<Type, Type>, Action<Object>> _backwardNavActionDictionary;
        private ICustomPresenter _presenter;
        IStartupService _startupService;
        IRepositories _repositories;
        private readonly IGatewayPollingService _gatewayPollingService;
        private readonly IMobileApplicationDataChunkService _mobileApplicationDataChunkService;
        ICloseApplication _closeApplication;
        private NavItem<MobileData> _mobileDataNavItem;
        private MobileData _mobileData;

        #endregion

        #region Construction

        public NavigationService(ICustomPresenter presenter, IStartupService startupService, ICloseApplication closeApplication, IRepositories repositories, IGatewayPollingService gatewayPollingService, IMobileApplicationDataChunkService mobileApplicationDataChunkService)
        {
            _forwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<Object>>();
            _backwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action<Object>>();
            _presenter = presenter;

            _mobileApplicationDataChunkService = mobileApplicationDataChunkService;
            _repositories = repositories;
            _gatewayPollingService = gatewayPollingService;
            _startupService = startupService;
            _closeApplication = closeApplication;

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


        public void InsertCustomNavAction<T1, T2>(Action<Object> action)
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


        public void InsertCustomBackNavAction<T1, T2>(Action<Object> action)
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


        public Action<Object> GetNavAction<T1, T2>()
            where T1 : BaseActivityViewModel
            where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action<Object> GetNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action<Object> GetBackNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_backwardNavActionDictionary, key);
        }

        #endregion

        #region INavigationService

        public void MoveToNext()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<Object> navAction = this.GetNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownNavigationMappingException(currentActivityType, currentFragmentType);

            navAction.Invoke(null);

        }

        public void MoveToNext(Object parameters)
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<Object> navAction = this.GetNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownNavigationMappingException(currentActivityType, currentFragmentType);

            navAction.Invoke(parameters);

        }

        public void GoBack()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action<Object> navAction = this.GetBackNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownBackNavigationMappingException(currentActivityType, currentFragmentType);

            navAction.Invoke(null);

        }

        public bool IsBackActionDefined()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            return BackNavActionExists(currentActivityType, currentFragmentType);
        }

        #endregion

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

        #endregion

        #region Private Methods

        private void MoveTo(Type type, Object parameters)
        {
            this.ShowViewModel(type, parameters);
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



        private Action<Object> GetNavActionWithKey(Dictionary<Tuple<Type, Type>, Action<Object>> dictionary, Tuple<Type, Type> key)
        {
            Action<Object> action = null;
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

        private void CompleteInstruction(MobileData mobileDataContent)
        {
            Mvx.Resolve<IUserInteraction>().Confirm("Do you wish to complete?", isConfirmed =>
            {
                if (isConfirmed)
                {
                    mobileDataContent.ProgressState = Enums.InstructionProgress.Complete;
                    _mobileApplicationDataChunkService.CurrentMobileData = mobileDataContent;
                    _mobileApplicationDataChunkService.Commit();
                    this.ShowViewModel<MainViewModel>();
                }
            }, "Complete Instruction", "Confirm", "Cancel");
        }

        private void GetMobileDataContent(Object parameters, out NavItem<MobileData> navItem, out MobileData mobileDataContent)
        {
            navItem = (parameters as NavItem<MobileData>);
            mobileDataContent = _repositories.MobileDataRepository.GetByID(navItem.ID);
        }

        private void DriverLogIn()
        {
            _startupService.DriverLogIn();
            _gatewayPollingService.StartPollingTimer();
        }

        #endregion

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
            InsertCustomNavAction<StartupViewModel, SafetyCheckSignatureViewModel>(Signature_CustomAction); //to either main activity (manifest) or back to driver passcode

            InsertCustomBackNavAction<StartupViewModel, PasscodeViewModel>(CloseApplication);               //Back from passcode closes app

            // Main Activity
            InsertCustomBackNavAction<MainViewModel, ManifestViewModel>(Manifest_CustomBackAction); // Back from manifest sends back to startup activity
            InsertCustomNavAction<MainViewModel, ManifestViewModel>(Manifest_CustomAction);

            InsertBackNavAction<MainViewModel, InstructionViewModel>(typeof(ManifestViewModel));
            InsertCustomNavAction<MainViewModel, InstructionViewModel>(Instruction_CustomAction);

            InsertBackNavAction<MainViewModel, InstructionOnSiteViewModel>(typeof(ManifestViewModel));
            InsertCustomNavAction<MainViewModel, InstructionOnSiteViewModel>(InstructionOnSite_CustomAction);

            //TODO: Implement back button
            InsertCustomNavAction<MainViewModel, InstructionTrailerViewModel>(InstructionTrailer_CustomAction);

            //TODO: Implement back button
            InsertCustomNavAction<MainViewModel, InstructionCommentViewModel>(InstructionComment_CustomAction);

            InsertCustomNavAction<MainViewModel, InstructionSignatureViewModel>(InstructionSignature_CustomAction);

        }

        #endregion

        #region Custom Mapping Actions

        private void CloseApplication(Object parameters)
        {
            _closeApplication.CloseApp();
        }

        /// <summary>
        /// Safety Check screen goes to main activity (manifest) if there are no profiles
        /// or odometer screen if odometer reading is required, safety check signature screen otherwise
        /// </summary>
        public void SafetyCheck_CustomAction(Object parameters)
        {

            if (VehicleSafetyProfile == null && TrailerSafetyProfile == null)
            {
                this.DriverLogIn();
                this.ShowViewModel<MainViewModel>();
            }
            else
            {
                if (VehicleSafetyProfile.OdometerRequired)
                    this.ShowViewModel<OdometerViewModel>();
                else
                    this.ShowViewModel<SafetyCheckSignatureViewModel>();
            }

        }

        /// <summary>
        /// Signature screen goes back to driver pass code screen if we have any safety check failures
        /// and to main acticity (manifest) otherwise
        /// </summary>
        public void Signature_CustomAction(Object parameters)
        {

            if (SafetyCheckStatus == Enums.SafetyCheckStatus.Failed)
            {
                ChangePresentation(new Presentation.CloseUpToViewPresentationHint(typeof(PasscodeViewModel)));
            }
            else
            {
                this.DriverLogIn();
                this.ShowViewModel<MainViewModel>();
            }

        }

        /// <summary>
        /// Manifest screen goes back to to instruction screen if we get a mobile data nav item
        /// and to main acticity (manifest) otherwise
        /// </summary>
        public void Manifest_CustomAction(Object parameters)
        {

            if (parameters is NavItem<MobileData>)
            {
                this.ShowViewModel<InstructionViewModel>(parameters as NavItem<MobileData>);
            }
            // else ??

        }

        public void Manifest_CustomBackAction(Object parameters)
        {
            // Stop the gateway polling service before we "logout" the user.
            _gatewayPollingService.StopPollingTimer();
            MoveTo(typeof(StartupViewModel), parameters);
        }


        /// <summary>
        /// Instruction screen. If we're getting an "item" (order) then show the order in question
        /// If we're getting a mobile data then move on to InstructionOnSiteViewModel
        /// </summary>
        /// <param name="parameters"></param>
        public void Instruction_CustomAction(Object parameters)
        {
            if (parameters is NavItem<Item>)
            {
                ShowViewModel<OrderViewModel>(parameters);
            }
            else if (parameters is NavItem<Models.Instruction.Trailer>)
            {
                ShowViewModel<InstructionTrailerViewModel>(parameters);
            }
            else if (parameters is NavItem<MobileData>)
            {
                var mobileDataNav = (NavItem<MobileData>)parameters;
                var mobileData = _repositories.MobileDataRepository.GetByID(mobileDataNav.ID);
                _mobileApplicationDataChunkService.CurrentMobileData = mobileData;
                _mobileApplicationDataChunkService.Commit();

                ShowViewModel<InstructionOnSiteViewModel>(parameters);
            }
        }

        /// <summary>
        /// Instruction on site screen, if the trailer selection is enabled then it will redirect to the trailer selection screen
        /// else if trailer selection is not enabled and the bypass comment screen is not then enabled then will it redirect to comment screen.
        /// else if trailer selection is not enabled and the bypass comment screen is enabled 
        /// and if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public void InstructionOnSite_CustomAction(Object parameters)
        {
            if (parameters is NavItem<Item>)
            {
                ShowViewModel<OrderViewModel>(parameters);
            }
            else if (parameters is NavItem<MobileData>)
            {

                GetMobileDataContent(parameters, out _mobileDataNavItem, out _mobileData);

                var additionalContent = _mobileData.Order.Additional;
                var itemAdditionalContent = _mobileData.Order.Items.First().Additional;

                // Debug Code
                //additionalContent.IsTrailerConfirmationEnabled = true;
                //additionalContent.CustomerSignatureRequiredForCollection = true;
                //additionalContent.CustomerSignatureRequiredForDelivery = true;
                //itemAdditionalContent.BypassCommentsScreen = true;

                //Collection
                if (_mobileData.Order.Type == Enums.InstructionType.Collect)
                {
                    if (additionalContent.IsTrailerConfirmationEnabled)
                    {
                        this.ShowViewModel<InstructionTrailerViewModel>(_mobileDataNavItem);
                        return;
                    }
                }

                if (!itemAdditionalContent.BypassCommentsScreen)
                {
                    this.ShowViewModel<InstructionCommentViewModel>(_mobileDataNavItem);
                    return;
                }

                if (((additionalContent.CustomerNameRequiredForDelivery || additionalContent.CustomerSignatureRequiredForDelivery) && _mobileData.Order.Type == Enums.InstructionType.Deliver) || 
                    ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && _mobileData.Order.Type == Enums.InstructionType.Collect))
                {
                    this.ShowViewModel<InstructionSignatureViewModel>(_mobileDataNavItem);
                    return;
                }

                CompleteInstruction(_mobileData);

            }
        }


        /// <summary>
        /// Instruction trailer screen, if the bypass comment screen is not then enabled then will it redirect to comment screen.
        /// else if the bypass comment screen is enabled and if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public void InstructionTrailer_CustomAction(Object parameters)
        {
            if (parameters is NavItem<MobileData>)
            {
                GetMobileDataContent(parameters, out _mobileDataNavItem, out _mobileData);

                var additionalContent = _mobileData.Order.Additional;
                var itemAdditionalContent = _mobileData.Order.Items.First().Additional;


                if (!itemAdditionalContent.BypassCommentsScreen)
                {
                    this.ShowViewModel<InstructionCommentViewModel>(_mobileDataNavItem);
                    return;
                }

                if (additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection)
                {
                    this.ShowViewModel<InstructionSignatureViewModel>(_mobileDataNavItem);
                    return;
                }

                CompleteInstruction(_mobileData);
            }
            else if (parameters is NavItem<Models.Instruction.Trailer>)
            {
                this.ShowViewModel<InstructionViewModel>(parameters);
            }
        }

        /// <summary>
        /// Instruction comment screen, if either either name required or signature required are enabled then redirect to signature screen.
        /// </summary>
        public void InstructionComment_CustomAction(Object parameters)
        {
            if (parameters is NavItem<MobileData>)
            {
                GetMobileDataContent(parameters, out _mobileDataNavItem, out _mobileData);

                var additionalContent = _mobileData.Order.Additional;
                var itemAdditionalContent = _mobileData.Order.Items.First().Additional;

                if (((additionalContent.CustomerNameRequiredForDelivery || additionalContent.CustomerSignatureRequiredForDelivery) && _mobileData.Order.Type == Enums.InstructionType.Deliver) ||
                   ((additionalContent.CustomerNameRequiredForCollection || additionalContent.CustomerSignatureRequiredForCollection) && _mobileData.Order.Type == Enums.InstructionType.Collect))
                {
                    this.ShowViewModel<InstructionSignatureViewModel>(_mobileDataNavItem);
                    return;
                }

                CompleteInstruction(_mobileData);


            }
        }

        /// <summary>
        /// Instruction signature screen, complete instruction and go back to manifest screen
        /// </summary>
        public void InstructionSignature_CustomAction(Object parameters)
        {
            if (parameters is NavItem<MobileData>)
            {
                GetMobileDataContent(parameters, out _mobileDataNavItem, out _mobileData);
                CompleteInstruction(_mobileData);
            }
        }




        #endregion
        
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
