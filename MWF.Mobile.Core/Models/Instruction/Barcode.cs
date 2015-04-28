using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class Barcode : IBlueSphereEntity
    {

        public Barcode()
        {
            ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        [XmlIgnore]
        public Guid ID { get; set; }

        [JsonProperty("#text")]
        public string BarcodeText { get; set; }

        public bool IsScanned { get; set; }

        public int OrderID { get; set; }

        public string DeliveryStatusCode { get; set; }

        [ForeignKey(typeof(Item))]
        [XmlIgnore]
        public Guid ItemId { get; set; }
    }
}
