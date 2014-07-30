using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Presentation;


namespace MWF.Mobile.Core.Services
{

    public class NavigationService : MvxNavigatingObject
    {

        #region Private Members

        private Dictionary<Tuple<Type, Type>, Action> _navActionDictionary;
        private ICustomPresenter _presenter;

        #endregion

        #region Construction

        public NavigationService(ICustomPresenter presenter)
        {
            _navActionDictionary = new Dictionary<Tuple<Type, Type>, Action>();
            _presenter = presenter;
        }

        #endregion

        #region Public Methods

        public void InsertNavAction<T1,T2>(Type destinationNodeType) where T1 : MvxViewModel
                                                               where T2 : MvxViewModel
        {
            //todo: catch dest type not being an mvx view model
            var key = CreateKey<T1, T2>();
            _navActionDictionary.Add(key, () => MoveTo(destinationNodeType) );
        }


        public bool NavActionExists<T1, T2>()
            where T1 : MvxViewModel
            where T2 : MvxViewModel
        {
            var key = CreateKey<T1, T2>();
            return _navActionDictionary.ContainsKey(key);
        }


        public Action GetNavAction<T1, T2>()
            where T1 : MvxViewModel
            where T2 : MvxViewModel
        {
            var key = CreateKey<T1, T2>();
            return GetNavActionWithKey(key);
        }

        // todo: catch input typea not being an mvx view model
        public Action GetNavAction(Type activityType, Type fragmentType)
        {
            var key = CreateKey(activityType, fragmentType);
            return GetNavActionWithKey(key);
        }

        public void MoveToNextFrom(MvxViewModel currentViewModel)
        {

            Type currentActivityType = _presenter.CurrentActivityViewModel.GetType();
            Type currentFragmentType = currentViewModel.GetType();

            //todo: catch mapping not existing

            Action navAction = this.GetNavAction(currentActivityType, currentFragmentType);
            navAction.Invoke();

        }

        #endregion

        #region Properties

        #endregion

        #region Private Methods

        private void MoveTo(Type type)
        {
            this.ShowViewModel(type);
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

        private Action GetNavActionWithKey(Tuple<Type, Type> key)
        {
            Action action = null;
            _navActionDictionary.TryGetValue(key, out action);
            return action;
        }


        #endregion


    }

}
