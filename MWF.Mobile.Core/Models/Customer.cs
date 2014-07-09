using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace MWF.Mobile.Core.Models
{
    public class Customer
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Login { get; set; }
    }
}
