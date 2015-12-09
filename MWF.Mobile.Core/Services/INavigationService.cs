using System;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface INavigationService
    {
        Task GoBack();
        Task GoBack(NavData navData);
        bool IsBackActionDefined();
        Task MoveToNext();
        Task MoveToNext(NavData navData);
        Task GoToManifest();
        Task Logout_Action(NavData navData);
        void PopulateNavData(NavData navData);
        NavData CurrentNavData { get; }
        bool ShowModalViewModel<TViewModel, TResult>(BaseFragmentViewModel viewModel, NavData navData, Action<TResult> onResult) where TViewModel : IModalViewModel<TResult>;

    }
}
