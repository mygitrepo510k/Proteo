using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Presentation;


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

        private Dictionary<Tuple<Type, Type>, Action> _forwardNavActionDictionary;
        private Dictionary<Tuple<Type, Type>, Action> _backwardNavActionDictionary;
        private ICustomPresenter _presenter;
        IStartupService _startupService;
        IRepositories _repositories;
        ICloseApplication _closeApplication;

        #endregion

        #region Construction

        public NavigationService(ICustomPresenter presenter, IStartupService startupService, ICloseApplication closeApplication, IRepositories repositories)
        {
            _forwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action>();
            _backwardNavActionDictionary = new Dictionary<Tuple<Type, Type>, Action>();
            _presenter = presenter;

            _repositories = repositories;
            _startupService = startupService;
            _closeApplication = closeApplication;

            SetMappings();
        }

        #endregion

        #region Public Methods


        public void InsertNavAction<T1,T2>(Type destinationNodeType)
                where T1 : BaseActivityViewModel
                where T2 : BaseFragmentViewModel
        {

            if (!IsDestinationTypeValid(destinationNodeType)) throw new ArgumentException("destinationNodeType must derive from MvxViewModel");

            var key = CreateKey<T1, T2>();
            _forwardNavActionDictionary.Add(key, () => MoveTo(destinationNodeType) );
        }


        public void InsertCustomNavAction<T1, T2>(Action action) where T1 : BaseActivityViewModel
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
            _backwardNavActionDictionary.Add(key, () => MoveBackTo(destinationNodeType));
        }


        public void InsertCustomBackNavAction<T1, T2>(Action action)
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

            var key = CreateKey(activityType,  fragmentType);
            return _backwardNavActionDictionary.ContainsKey(key);
        }


        public Action GetNavAction<T1, T2>()
                            where T1 : BaseActivityViewModel
                where T2 : BaseFragmentViewModel
        {
            var key = CreateKey<T1, T2>();
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action GetNavAction(Type activityType, Type fragmentType)
        {
            if (!AreSourceTypesValid(activityType, fragmentType)) throw new ArgumentException("View model types must derive from BaseActivityviewModel, BaseFragmentViewModel");

            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(_forwardNavActionDictionary, key);
        }


        public Action GetBackNavAction(Type activityType, Type fragmentType)
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

            Action navAction = this.GetNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownNavigationMappingException(currentActivityType, currentFragmentType);

            navAction.Invoke();

        }

        public void GoBack()
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = _presenter.CurrentFragmentViewModel.GetType();

            Action navAction = this.GetBackNavAction(currentActivityType, currentFragmentType);

            if (navAction == null) throw new UnknownBackNavigationMappingException(currentActivityType, currentFragmentType);

            navAction.Invoke();

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

        private void MoveTo(Type type)
        {
            this.ShowViewModel(type);
        }

        private void MoveBackTo(Type type)
        {
            ChangePresentation(new Presentation.CloseUpToViewPresentationHint(type));
        }

        private Tuple<Type,Type> CreateKey<T1, T2>()
            where T1 : MvxViewModel
            where T2 : MvxViewModel
        {
            return Tuple.Create<Type, Type>(typeof(T1), typeof(T2));
        }

        private Tuple<Type, Type> CreateKey(Type activityType, Type fragmentType)
        {
            return Tuple.Create<Type, Type>(activityType, fragmentType);
        }

        private Action GetNavActionWithKey(Dictionary<Tuple<Type, Type>, Action> dictionary, Tuple<Type, Type> key)
        {
            Action action = null;
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
            InsertCustomBackNavAction<MainViewModel, ManifestViewModel>(() => MoveTo(typeof(StartupViewModel))); // Back from manifest sends back to startup activity
        }

        #endregion

        #region Custom Mapping Actions

        private void CloseApplication()
        {
            _closeApplication.CloseApp();
        }

        /// <summary>
        /// Safety Check screen goes to main activity (manifest) if there are no profiles
        /// or odometer screen if odometer reading is required, safety check signature screen otherwise
        /// </summary>
        public void SafetyCheck_CustomAction()
        {

            if (VehicleSafetyProfile == null && TrailerSafetyProfile == null)
            {
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
        public void Signature_CustomAction()
        {

            if (SafetyCheckStatus == Enums.SafetyCheckStatus.Failed)
            {
                ChangePresentation(new Presentation.CloseUpToViewPresentationHint(typeof(PasscodeViewModel)));
            }
            else
            {
                this.ShowViewModel<MainViewModel>();
            }

        }


        #endregion


    }

    #region Exception Classes

    public class UnknownNavigationMappingException : Exception
    {
        public UnknownNavigationMappingException(Type activityType, Type fragmentType) : base(string.Format("No mapping defined for {0} activity / {1} fragment", activityType, fragmentType))
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
