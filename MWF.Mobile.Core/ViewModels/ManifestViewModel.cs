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
            var section1 = new Section()
            {
                Header = "Active Instructions",
                Instructions = Enumerable.Repeat<MobileApplicationData>(testOrder, 5)
            };
            var section2 = new Section()
            {
                Header = "Instruction",
                Instructions = Enumerable.Repeat<MobileApplicationData>(testOrder, 5)
            };
            Sections = new List<Section>() { section1, section2 }.AsEnumerable<Section>();
        }

        public override string FragmentTitle
        {
            get { return "Manifest"; }
        }

        public string HeaderText
        {
            get { return "Select instructions - Showing " + InsructionsCount; }
        }


        private IEnumerable<Section> _sections;
        public IEnumerable<Section> Sections
        {
            get { return _sections; }
            set { _sections = value; RaisePropertyChanged(() => Sections); }
        }

        public string _dateOutput;
        public string DateOutput
        {
            get { return _dateOutput; }
            set { _dateOutput = value; RaisePropertyChanged(() => DateOutput); }
        }

        public int InsructionsCount
        {
            get { return Sections.Sum(s => s.Count()); }
        }
    }

    public class Section : IEnumerable<MobileApplicationData>
    {

        public string Header { get; set; }
        public IEnumerable<MobileApplicationData> Instructions { get; set; }

        public IEnumerator<MobileApplicationData> GetEnumerator()
        {
            return Instructions.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
