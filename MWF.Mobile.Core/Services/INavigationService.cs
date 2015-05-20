using System;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;

namespace MWF.Mobile.Core.Services
{
    public interface INavigationService
    {
        void GoBack();
        void GoBack(NavData navData);
        bool IsBackActionDefined();
        void MoveToNext();
        void MoveToNext(NavData navData);
        void GoToManifest();
        void Logout_Action(NavData navData);
        void PopulateNavData(NavData navData);
        NavData CurrentNavData { get; }
        bool ShowModalViewModel<TViewModel, TResult>(BaseFragmentViewModel viewModel, NavData navData, Action<TResult> onResult) where TViewModel : IModalViewModel<TResult>;

    }
}
