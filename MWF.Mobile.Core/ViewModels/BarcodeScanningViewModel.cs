using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels.Interfaces;
using MWF.Mobile.Core.ViewModels.Navigation.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MWF.Mobile.Core.ViewModels
{
    public class BarcodeScanningViewModel :
        BaseInstructionNotificationViewModel,
        IVisible
    {
        #region Construction

        private MobileData _mobileData = null;
        private NavData<MobileData> _navData;

        private BarcodeSectionViewModel _unprocessedBarcodes;
        private BarcodeSectionViewModel _processedBarcodes;
        private IRepositories _repositories;
        private List<DamageStatus> _damageStatuses;

        private INavigationService _navigationService;

        public BarcodeScanningViewModel(INavigationService navigationService, IRepositories repositories)
        {
            _navigationService = navigationService;
            _repositories = repositories;

            BuildDamageStatuses();

        }

        public void Init(NavData<MobileData> navData)
        {
            navData.Reinflate();
            _navData = navData;
            _mobileData = navData.Data;

            CreateSections();
        }

        private void CreateSections()
        {
            BarcodeSections = new ObservableCollection<BarcodeSectionViewModel>();

            _unprocessedBarcodes = new BarcodeSectionViewModel(this)
            {
                SectionHeader = "Unprocessed Barcodes",
            };

            BarcodeSections.Add(_unprocessedBarcodes);


            _processedBarcodes = new BarcodeSectionViewModel(this)
            {
                SectionHeader = "Processed Barcodes",
            };

            BarcodeSections.Add(_processedBarcodes);


            //Initalize the unprocessed collection
            List<BarcodeItemViewModel> barcodeItemsViewModels = new List<BarcodeItemViewModel>();
            var items = _mobileData.Order.Items;
            foreach (var item in items)
            {
                barcodeItemsViewModels.AddRange(item.BarcodesList.Select(b => new BarcodeItemViewModel(_navigationService, _damageStatuses, this) { BarcodeText = b, OrderID = item.ItemIdFormatted }));
            }


            foreach (var vm in barcodeItemsViewModels)
            {
                _unprocessedBarcodes.Barcodes.Add(vm);
            }

            RaisePropertyChanged(() => BarcodeSections);
        }

        #endregion Construction

        #region Public Properties


        private ObservableCollection<BarcodeSectionViewModel> _barcodeSections;
        public ObservableCollection<BarcodeSectionViewModel> BarcodeSections
        {
            get { return _barcodeSections; }
            set { _barcodeSections = value; RaisePropertyChanged(() => BarcodeSections); }
        }

        public IEnumerable<BarcodeItemViewModel> SelectedBarcodes
        {
            get
            {
                return _processedBarcodes.Where(x => x.IsSelected);
            }
        }


        private string _barcodeInput = string.Empty;
        public string BarcodeInput
        {
            get { return _barcodeInput; }
            set 
            {

                _barcodeInput = value; 
                CheckScanInput(); 
            }
        }

        private MvxCommand _completeScanningCommand;
        public ICommand CompleteScanningCommand
        {
            get
            {
                return (_completeScanningCommand = _completeScanningCommand ?? new MvxCommand(() => CompleteScanning()));
            }
        }

        public string InstructionsText
        {
            get
            {
                return "Scan barcodes or select them from the unprocessed list to process them.";
            }
        }


        public string CompleteButtonText
        {
            get
            {
                return "Continue";
            }
        }

        private bool _canScanningBeCompleted;
        public bool CanScanningBeCompleted
        {
            get
            {
                return _canScanningBeCompleted;

            }
            set
            {
                _canScanningBeCompleted = value;
                RaisePropertyChanged(() => CanScanningBeCompleted);
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        public string ScanTitle
        {
            get { return "Processing..."; }
        }

        public string ScanMessage
        {
            get { return "Processing..."; }
        }

        #endregion Public Properties

        #region Public Methods

        public void MarkBarcodeAsProcessed(BarcodeItemViewModel barcodeItem, bool wasScanned = true)
        {
            barcodeItem.IsScanned = wasScanned;
            barcodeItem.IsDelivered = true;

            _unprocessedBarcodes.Barcodes.Remove(barcodeItem);
            _processedBarcodes.Barcodes.Add(barcodeItem);

            if (_unprocessedBarcodes.Barcodes.Count == 0)
            {
                _unprocessedBarcodes.Barcodes.Add(new DummyBarcodeItemViewModel());
                this.CanScanningBeCompleted = true;
            }

            RaisePropertyChanged(() => BarcodeSections);

        }

        #endregion

        #region Private Methods


        private void BuildDamageStatuses()
        {
            _damageStatuses = new List<DamageStatus>();

            _damageStatuses.Add(new DamageStatus() { Text = "Clean", Code = "POD" });

            var palletForceVerbProfile = _repositories.VerbProfileRepository.GetAll().Where(vp => vp.Code == "PFORCE").FirstOrDefault();

            if (palletForceVerbProfile != null)
            {
                var customDamageStatuses = palletForceVerbProfile.Children.Select(vp => new DamageStatus() { Text = vp.Title, Code = vp.Category });
                _damageStatuses.AddRange(customDamageStatuses);
            }
        }

        private async void CheckScanInput()
        {
            var barcode = _unprocessedBarcodes.FirstOrDefault(bc => bc.BarcodeText == this.BarcodeInput);

            if (barcode !=null)
            {
                MarkBarcodeAsProcessed(barcode);
                ClearBarcode();
                RegainFocusViaHack();
            }
            else
            {
                ShowErrorAlert();
            }
               
        }

        private void ShowErrorAlert()
        {
            string errorMessage = "Invalid Barcode";

            if (_processedBarcodes.Barcodes.Any(x => x.BarcodeText == BarcodeInput))
                errorMessage = "Barcode already scanned";

            Mvx.Resolve<IUserInteraction>().Alert(errorMessage, () => { ClearBarcode(); RequestBarcodeFocus(); });
        }

        private async void RegainFocusViaHack()
        {
            this.IsBusy = true;
            await Task.Delay(200);
            this.IsBusy = false;
            RequestBarcodeFocus();
        }

       
        private void CompleteScanning()
        {

            var newScannedDelivery = new ScannedDelivery()
            { Barcodes = _processedBarcodes.Barcodes.Select(bvm => 
                            new Barcode 
                            { 
                                BarcodeText = bvm.BarcodeText, 
                                IsScanned = bvm.IsScanned, 
                                OrderID = bvm.OrderID,
                                IsDelivered = bvm.IsDelivered.Value,
                                DamageStatusCode = bvm.DamageStatus.Code,
                                DeliveryStatusCode = bvm.PalletforceDeliveryStatus,
                                DeliveryStatusNote = bvm.DeliveryComments
                                
                            }).ToList() 
            };

            _navData.GetDataChunk().ScannedDelivery = newScannedDelivery;

            _navigationService.MoveToNext(_navData);
        }

        private void ClearBarcode()
        {
            _barcodeInput = string.Empty;
            RaisePropertyChanged(() => BarcodeInput);
        }

        private void RequestBarcodeFocus()
        {
            RaisePropertyChanged("RequestBarcodeFocus");
        }


        #endregion Private Methods



        #region IVisible

        public void IsVisible(bool isVisible)
        {
            if (isVisible) { }
            else
            {
                this.UnsubscribeNotificationToken();
            }
        }

        #endregion IVisible

        #region BaseInstructionNotificationViewModel Overrides

        public override void CheckInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand notificationType, Guid instructionID)
        {

        }

        #endregion BaseInstructionNotificationViewModel Overrides

        #region BaseFragmentViewModel Overrides

        public override string FragmentTitle
        {
            get { return _mobileData.Order.Type.ToString() + " Scan"; }
        }

        #endregion BaseFragmentViewModel Overrides
    }

}
