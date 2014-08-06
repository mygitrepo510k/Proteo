using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class OrderViewModel
        : BaseFragmentViewModel
    {
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;
        private Item _order;

        #endregion

        #region Construction

        public OrderViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;
        }

        public void Init(NavItem<Item> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ParentID);
            _order = _mobileData.Order.Items.FirstOrDefault(i => i.ID == item.ID);
        }


        #endregion

        #region Public Properties

        public string OrderName { get { return _order.ItemId; } }

        #endregion

        #region Private Methods

        #endregion

        #region BaseFragmentViewModel Overrides
        public override string FragmentTitle
        {
            get { return "Order"; }
        }

        #endregion

    }
}
