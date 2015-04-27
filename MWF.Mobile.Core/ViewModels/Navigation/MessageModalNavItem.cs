using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class MessageModalNavItem : IModalNavItem
    {
        public Guid MessageID { get; set; }
        public Guid MobileDataID { get; set; }
        public bool IsRead { get; set; }
    }
}
