using System;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Core.ViewModels.Interfaces;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface INavigationService
    {
        Task GoBackAsync();
        Task GoBackAsync(NavData navData);
        bool IsBackActionDefined();
        Task MoveToNextAsync();
        Task MoveToNextAsync(NavData navData);
        Task GoToManifestAsync();
        Task Logout_ActionAsync(NavData navData);
        void PopulateNavData(NavData navData);
        NavData CurrentNavData { get; }
        bool ShowModalViewModel<TViewModel, TResult>(BaseFragmentViewModel viewModel, NavData navData, Action<TResult> onResult) where TViewModel : IModalViewModel<TResult>;

    }
}
