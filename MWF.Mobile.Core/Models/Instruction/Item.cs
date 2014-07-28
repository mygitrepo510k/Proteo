using System.Collections.Generic;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Item
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DeliveryOrderNumber { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string OrderId { get; set; }
        public string Quantity { get; set; }
        public string Weight { get; set; }
        public string Unit { get; set; }
        public string Price { get; set; }
        public string DeliveryType { get; set; }
        public string BusinessType { get; set; }
        public Additional Additional { get; set; }
        public List<Barcode> Barcodes { get; set; }
    }
}