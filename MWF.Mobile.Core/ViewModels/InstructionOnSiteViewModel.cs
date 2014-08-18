using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class InstructionOnSiteViewModel : BaseFragmentViewModel
    {

        
        #region Private Fields

        private readonly INavigationService _navigationService;
        private readonly IRepositories _repositories;
        private MobileData _mobileData;


        #endregion

        #region Construction

        public InstructionOnSiteViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;                 
        }

        public void Init(NavItem<MobileData> item)
        {
            _mobileData = _repositories.MobileDataRepository.GetByID(item.ID);
            _orderList = new ObservableCollection<Item>(_mobileData.Order.Items);          
        }

        #endregion

        #region Public Properites

        private MvxCommand _advanceInstructionOnSiteCommand;
        public ICommand AdvanceInstructionOnSiteCommand
        {
            get
            {
                return (_advanceInstructionOnSiteCommand = _advanceInstructionOnSiteCommand ?? new MvxCommand(() => AdvanceInstructionOnSite()));
            }
        }

        private MvxCommand<Item> _showInstructionOrderCommand;
        public ICommand ShowInstructionOrderCommand
        {
            get
            {
                return(_showInstructionOrderCommand = _showInstructionOrderCommand ?? new MvxCommand<Item>(o => ShowInstructionOrder(o)));
            }
        }

        private ObservableCollection<Item> _orderList;
        public ObservableCollection<Item> OrderList
        {
            get { return _orderList; }
            set { _orderList = value; RaisePropertyChanged(() => OrderList); }
        }

        public string InstructionCommentButtonLabel { get { return "Move on"; } }

        public string HeaderText { get { return "Select an order for further details"; } }

        #endregion

        #region Private Methods

        private void AdvanceInstructionOnSite()
        {
            NavItem<MobileData> navItem = new NavItem<MobileData>() { ID = _mobileData.ID };
            _navigationService.MoveToNext(navItem);
        }

        private void ShowInstructionOrder(Item order)
        {
            Mvx.Resolve<IUserInteraction>().Alert(order.ItemIdFormatted, null, "Order Details");
        }

        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "On Site"; }
        }

        #endregion

    }
}
