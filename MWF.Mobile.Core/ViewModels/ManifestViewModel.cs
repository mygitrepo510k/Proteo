using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System.Collections.Generic;
using MWF.Mobile.Core.Models;
using System.Linq;
using System;
using MWF.Mobile.Core.Models.Instruction;

namespace MWF.Mobile.Core.ViewModels
{

    public class ManifestViewModel 
		: BaseFragmentViewModel
    {

        public ManifestViewModel()
        {
            var testOrder = new Order() { Arrive = DateTime.Now.AddMonths(1), Description = "test" };
            MobileApplicationData = Enumerable.Repeat<Order>(testOrder, 1);
        }

        public override string FragmentTitle
        {
            get { return "Manifest"; }
        }

        private IEnumerable<Order> _mobileApplicationData;
        public IEnumerable<Order> MobileApplicationData
        {
            get { return _mobileApplicationData; }
            set { _mobileApplicationData = value; RaisePropertyChanged(() => MobileApplicationData); }
        }

        public string _dateOutput;
        public string DateOutput
        {
            get { return _dateOutput; }
            set { _dateOutput = value; RaisePropertyChanged(() => DateOutput); }
        }
    }

}
