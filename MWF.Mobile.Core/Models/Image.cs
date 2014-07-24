using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models
{
    // Model class which holds an image associated with a safety check fault
    public class Image: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [XmlAttribute("id")]
        public Guid ID { get; set; }

        [XmlAttribute("sequence")]
        public int Sequence { get; set; }

        [XmlAttribute("encoded")]
        public string EncodeImageData { get; set; }

        [ForeignKey(typeof(SafetyCheckFault))]
        [XmlIgnore]
        public Guid SafetyCheckFaultID { get; set; }

    }
}
