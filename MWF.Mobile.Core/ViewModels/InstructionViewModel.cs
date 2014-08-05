using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public string OrderID { get { return _mobileData.Order.OrderId; } }

        public string Address { get { return _mobileData.Order.Addresses[0].Lines + "\n" + _mobileData.Order.Addresses[0].Postcode; } }

        public string Notes { get { return string.Empty; } }

        public IList<Item> Orders { get { return _mobileData.Order.Items; } }

        public string Trailer { get { return (_mobileData.Order.Additional.Trailer == null) ? string.Empty : _mobileData.Order.Additional.Trailer.DisplayName; } }

        public string DepartLabelText { get { return "Depart"; } }
        
        //public string DepartLabelText { get { return "Depart"; } }

        public string InstructionButtonLabel { get { return "Move on"; } }

        #endregion


        #region Private Properties

        private MvxCommand _advanceInstructionCommand;
        public ICommand AdvanceInstructionCommand
        {
            get
            {
                return (_advanceInstructionCommand = _advanceInstructionCommand ?? new MvxCommand(() => AdvanceInstruction()));
            }
        }

        public void AdvanceInstruction()
        {
            _navigationService.MoveToNext();
        }


        #endregion

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return "Collect"; }
        }

        #endregion


        private MobileData BuildMobileData()
        {

            Address address = new Address()
            {
                Country =  "United Kingdom",
                Lines = "21 Mornington Road/nNorwich.nNorfolk",
                Postcode = "NR2 3NA"
            };

            Item item1 = new Item()
            {
               ItemId = "Order10241"
            };

            Item item2 = new Item()
            {
                ItemId = "Order10242"
            };


            Trailer trailer = new Trailer(){
                DisplayName= "Trailer"
            };

            Additional additional = new Additional()
            {
                Trailer = trailer
            };

            Order order = new Order()
            {
                Type = Enums.InstructionType.Collect,                   //instruction type
                RouteId = "Run10226",
                OrderId = "10241",
                Arrive = DateTime.Now,
                Depart = DateTime.Now.AddMinutes(15),
                Addresses = new List<Address>() { address },
                Items = new List<Item> { item1, item2},
                Additional =  additional

            };
    

            MobileData data = new MobileData() {
                       CustomerId= Guid.Parse("c697166b-2e1b-45b0-8f77-270c4eadc031"),
                       Order = order,
                       ProgressState = Enums.InstructionProgress.NotStarted

            };

            return data;

        }


    }
}
