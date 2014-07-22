using Cirrious.MvvmCross.Plugins.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IGpsService
    {
        string GetSmpData(ReportReason reportReason);
    }
}
