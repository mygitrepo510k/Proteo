using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models
{
    // Model class which represents a fault for a specific safety check item 9SafetyCheckFaultType)
    public class SafetyCheckFault: IBlueSphereEntity
    {
        [Unique]
        [PrimaryKey]
        [XmlAttribute("id")]
        public Guid ID { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("faultid")]
        public Guid FaultTypeID { get; set; }

        [XmlAttribute("reference")]
        public string FaultTypeReference { get; set; }

        [XmlAttribute("comment")]
        public string Comment { get; set; }

        [JsonProperty("@discrete")]
        public bool IsDiscretionaryPass { get; set; }

        [ChildRelationship(typeof(Image))]
        [XmlArray("images")]
        public List<Image> Images { get; set; }

        [ForeignKey(typeof(SafetyCheckData))]
        [XmlIgnore]
        public Guid SafetyCheckDataID { get; set; }

    }
}
