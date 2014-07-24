using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class SafetyCheckNavItem : IModalNavItem
    {
        public Guid MessageID { get; set; }
        public Guid FaultID { get; set; }
        public bool IsVehicle { get; set; }
        public string FaultTypeText { get; set; }
    }
}
