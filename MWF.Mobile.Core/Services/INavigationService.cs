using System;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Core.Services
{
    public interface INavigationService
    {
        void GoBack();
        void GoBack(Object parameters);
        bool IsBackActionDefined();
        void MoveToNext();
        void MoveToNext(Object parameters);
    }
}
