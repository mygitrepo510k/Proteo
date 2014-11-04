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

        public string OrderName { get { return "Order " + _order.ItemIdFormatted; } }

        public string OrderID { get { return _order.ItemIdFormatted; } }

        public string OrderLoadNo { get { return _order.Title; } }

        public string OrderDeliveryNo { get { return _order.DeliveryOrderNumber; } }

        public string OrderQuantity { get { return _order.Quantity; } }

        public string OrderWeight { get { return _order.Weight; } }

        public string OrderBusinessType { get { return _order.BusinessType; } }

        public string OrderGoodsType { get { return _order.GoodsType; } }

        public string ChangeOrderQuantityButtonLabel { get { return "Change Quantity"; } }

        //TODO: Find the variable to allow user to alter order quantity
        public bool ChangeOrderQuantity { get { return true; } }



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
