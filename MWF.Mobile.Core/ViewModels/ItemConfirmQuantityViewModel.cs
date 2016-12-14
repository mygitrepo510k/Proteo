using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public class ItemConfirmQuantityViewModel : BaseFragmentViewModel
    {
        public ItemConfirmQuantityViewModel(){}

        private Item _item = null;
        private Enums.InstructionType _instructionType;
        public ItemConfirmQuantityViewModel(Item item, Enums.InstructionType instructionType )
        {
            _item = item;
            _instructionType = instructionType;
        }
        public override string FragmentTitle
        {
            get { return "Confirm Quantity"; }
        }

        #region Properties
        private string _cases;
        public string Cases
        {
            get { return _cases; }
            set { _cases = value; RaisePropertyChanged(() => Cases); }
        }
        private string _Pallets;
        public string Pallets
        {
            get { return _Pallets; }
            set { _Pallets = value; RaisePropertyChanged(() => Pallets); }
        }

        private string _weight;
        public string Weight
        {
            get { return _weight; }
            set { _weight = value; RaisePropertyChanged(() => Weight); }
        }

        private string _other;
        public string Other
        {
            get { return _other; }
            set { _other = value; RaisePropertyChanged(() => Other); }
        }

        public bool ConfirmCases
        {
            get { return (_instructionType == Enums.InstructionType.Collect && _item.ConfirmCasesForCollection) || (_instructionType == Enums.InstructionType.Deliver && _item.ConfirmCasesForDelivery) ; }
        }
        public bool ConfirmPallets
        {
            get { return (_instructionType == Enums.InstructionType.Collect && _item.ConfirmPalletsForCollection) || (_instructionType == Enums.InstructionType.Deliver && _item.ConfirmPalletsForDelivery); }
        }
        public bool ConfirmWeight
        {
            get { return (_instructionType == Enums.InstructionType.Collect && _item.ConfirmWeightForCollection) || (_instructionType == Enums.InstructionType.Deliver && _item.ConfirmWeightForDelivery); }
        }
        public bool ConfirmOther
        {
            get { return (_instructionType == Enums.InstructionType.Collect && _item.ConfirmOtherForCollection) || (_instructionType == Enums.InstructionType.Deliver && _item.ConfirmOtherForDelivery); }
        }

        public string ConfirmOtherText
        {
            get { return (_instructionType == Enums.InstructionType.Collect ? _item.ConfirmOtherTextForCollection : _item.ConfirmOtherTextForDelivery); }
        }

        public string ConfirmQuantityTitle
        {
            get { return _item.DeliveryOrderNumber ?? _item.ItemIdFormatted; }
        }

        public bool IsClaused
        {
        get {
                bool isClaused = false;
                if (ConfirmCases)
                    isClaused = (Cases != _item.Cases);
                if (ConfirmPallets && !isClaused)
                    isClaused = (Pallets != _item.Pallets);
                if (ConfirmWeight && !isClaused)
                    isClaused = (Weight != _item.Weight);
                return isClaused;
            }
        }
        #endregion

        #region provate methods
        private bool CanContinue()
        {
            bool retVal = (ConfirmCases && !string.IsNullOrEmpty(Cases));
            retVal = retVal & (ConfirmPallets && !string.IsNullOrEmpty(Pallets));
            retVal = retVal & (ConfirmWeight && !string.IsNullOrEmpty(Weight));
            retVal = retVal & (ConfirmOther && !string.IsNullOrEmpty(Other));

            
            return retVal;
        }


        #endregion
    }
}
