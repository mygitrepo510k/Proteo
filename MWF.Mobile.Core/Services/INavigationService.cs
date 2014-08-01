using System;
namespace MWF.Mobile.Core.Services
{
    public interface INavigationService
    {
        void GoBack();
        bool IsBackActionDefined();
        void MoveToNext();
    }
}
