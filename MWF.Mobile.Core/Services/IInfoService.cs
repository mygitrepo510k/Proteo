using System;
using MWF.Mobile.Core.Models;


namespace MWF.Mobile.Core.Services
{
    public interface IInfoService
    {
        Driver LoggedInDriver { get; set; }
    }
}
