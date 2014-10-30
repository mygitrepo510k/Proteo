using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models.Instruction
{
    [XmlRoot("mobiledatachunk")]
    public class MobileApplicationDataChunk : IBlueSphereEntity
    {
        public MobileApplicationDataChunk()
        {
            ID = new Guid();
        }

        [Unique]
        [PrimaryKey]
        [XmlAttribute("id")]
        public Guid ID { get; set; }

        [XmlAttribute("dataid")]
        public Guid MobileApplicationDataID { get; set; }

        [XmlElement("order")]
        public MobileApplicationDataChunkContentOrder Data { get; set; }

        [XmlIgnore]
        public SyncState SyncState { get; set; }


        [XmlAttribute("syncstate")]
        public int SyncStateInt
        {
            get { return (int)SyncState; }
            set { SyncState = (Enums.SyncState)value; }
        }

        [XmlAttribute("isdeleted")]
        public bool IsDeleted { get; set; }

        [XmlAttribute("sequence")]
        public int Sequence { get; set; }

       // [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("effectivedate")]
        public DateTime EffectiveDate { get; set; }
    }
}
