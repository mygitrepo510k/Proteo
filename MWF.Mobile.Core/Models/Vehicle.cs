using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{

    public class Vehicle
    {
        
        public int ID { get; set; }
        public string Registration { get; set; }
        public string Title { get; set; }
        public bool IsTrailer { get; set; }
        public int SafetyProfile { get; set; }
    }
}
