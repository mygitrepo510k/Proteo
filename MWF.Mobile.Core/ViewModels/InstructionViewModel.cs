using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MWF.Mobile.Core.Extensions;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.Repositories;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;

        #endregion

        #region Construction

        public InstructionViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavItem<MobileData> item)
        {        
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
        }

        #endregion

        #region Public Properties

        public string RunID { get { return _mobileData.GroupTitleFormatted; } }

        public string ArriveDateTime { get { return _mobileData.Order.Arrive.ToStringIgnoreDefaultDate(); } }

        public string DepartDateTime { get { return _mobileData.Order.Depart.ToStringIgnoreDefaultDate(); } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines.Replace("|","\n") + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string Notes { get { return string.Empty; } }

        public IList<Item> Orders { get { return _mobileData.Order.Items; } }

        public string Trailer { get { return (_mobileData.Order.Additional.Trailer == null) ? string.Empty : _mobileData.Order.Additional.Trailer.DisplayName; } }

        public string ArriveLabelText { get { return "Arrive"; } }

        public string DepartLabelText { get { return "Depart"; } }

        public string AddressLabelText { get { return "Address"; } }

        public string NotesLabelText { get { return "Notes"; } }

        public string OrdersLabelText { get { return "Orders"; } }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Collect"; }
        }

        #endregion



    }
}
