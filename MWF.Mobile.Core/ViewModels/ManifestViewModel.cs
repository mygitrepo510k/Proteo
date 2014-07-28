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
            var testOrder = new MobileApplicationData() { EffectiveDate = DateTime.Now.AddMonths(1), Title = "Proteo Test Client", VehicleRegistration = "243 234" };
            MobileApplicationData = Enumerable.Repeat<MobileApplicationData>(testOrder, 5);
        }

        public override string FragmentTitle
        {
            get { return "Manifest"; }
        }

        public string ManifestHeaderText
        {
            get { return "Select instructions - Showing " + MobileApplicationDataCount;  }
        }

        public int MobileApplicationDataCount
        {
            get { return MobileApplicationData.ToList().Count(); }
        }

        private IEnumerable<MobileApplicationData> _mobileApplicationData;
        public IEnumerable<MobileApplicationData> MobileApplicationData
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
