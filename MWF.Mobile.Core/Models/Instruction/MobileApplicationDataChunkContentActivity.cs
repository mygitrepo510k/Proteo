using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{
    public class MobileApplicationDataChunkContentActivity
    {
        public MobileApplicationDataChunkContentActivity()
        {
            Order = new List<Item>();
        }

        [XmlAttribute("drivertitle")]
        public Guid DriverId { get; set; }

        [XmlAttribute("vehiclereg")]
        public string VehicleRegistration { get; set; }

        [XmlAttribute("effectivedate")]
        public DateTime EffectiveDate { get; set; }

        [XmlAttribute("activity")]
        public int Activity { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("smp")]
        public string Smp { get; set; }

        [XmlAttribute("sequence")]
        public int Sequence { get; set; }

        [XmlAttribute("MWFVersion")]
        public string MwfVersion { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; }

        [XmlElement("signature")]
        public Signature Signature { get; set; }

        //This is used for sending back the updated order to bluesphere
        [XmlArray("data")]
        [XmlArrayItem(typeof(Item), ElementName = "item")]
        public List<Item> Order { get; set; }
    }
}
