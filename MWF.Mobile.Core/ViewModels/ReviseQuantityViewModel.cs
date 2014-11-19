﻿using Chance.MvvmCross.Plugins.UserInteraction;
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
    public class ReviseQuantityViewModel : BaseFragmentViewModel
    {

        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private readonly IUserInteraction _userInteraction;
        private MobileData _mobileData;
        private Item _order;

        #endregion Private Fields

        #region Construction

        public ReviseQuantityViewModel(INavigationService navigationService, IRepositories repositories, IUserInteraction userInteraction)
        {
            _navigationService = navigationService;
            _repositories = repositories;
            _userInteraction = userInteraction;
        }

        public void Init(NavItem<Item> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ParentID);
            _order = _mobileData.Order.Items.First(i => i.ID == item.ID);
            OrderQuantity = _order.Quantity;
        }

        #endregion Construction

        #region Public Properties

        public string ReviseQuantityButtonLabel{ get { return "Update Quantity"; } }

        public string ReviseQuantityHeaderLabel { get { return "Revise Quantity"; } }

        public string OrderName { get { return "Order " + _order.ItemIdFormatted; } }

        private string _orderQuantity;
        public string OrderQuantity
        {
            get { return _orderQuantity; }
            set { _orderQuantity = value; RaisePropertyChanged(() => OrderQuantity); }
        }

        #endregion Public Properties

        #region Public Methods

        private MvxCommand _reviseQuantityCommand;
        public ICommand ReviseQuantityCommand
        {
            get
            {
                return (_reviseQuantityCommand = _reviseQuantityCommand ?? new MvxCommand(() => ReviseQuantity()));
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void ReviseQuantity()
        {
            _order.Quantity = OrderQuantity;
            var orderID = _order.ID;

            foreach (var order in _mobileData.Order.Items)
            {
                if(order.ID == _order.ID)
                {
                    order.Quantity = OrderQuantity;
                }
            }

            var instructionToUpdate = _repositories.MobileDataRepository.GetByID(_mobileData.ID);
            if (instructionToUpdate != null)
            {
                var progress = instructionToUpdate.ProgressState;
                _repositories.MobileDataRepository.Delete(instructionToUpdate);
            }
            _repositories.MobileDataRepository.Insert(_mobileData);

            NavItem<Item> navItem = new NavItem<Item>() { ID = _order.ID, ParentID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);

        }

        #endregion

        public override string FragmentTitle
        {
            get { return "Revise Quantity"; }
        }
    }
}
