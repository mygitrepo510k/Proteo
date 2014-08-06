using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;

namespace MWF.Mobile.Core.ViewModels
{
    public class NavItem<T> where T: IBlueSphereEntity
    {
        public Guid ID { get; set; }
        public Guid ParentID { get; set; }
        public Type ModelType { get { return typeof(T); } }
    }
}
