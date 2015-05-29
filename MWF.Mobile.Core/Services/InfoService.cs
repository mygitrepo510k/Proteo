using System;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.Services
{
    public class InfoService : MWF.Mobile.Core.Services.IInfoService
    {
        public Driver LoggedInDriver { get; set; }
    }
}
