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

        private string _encodeImageData;
        private byte[] _bytes;


        [Unique]
        [PrimaryKey]
        [XmlAttribute("id")]
        public Guid ID { get; set; }

        [XmlAttribute("sequence")]
        public int Sequence { get; set; }

        [XmlAttribute("encoded")]
        public string EncodeImageData 
        {
            get { return _encodeImageData; }
            set
            {
                _encodeImageData = value;
                _bytes = Convert.FromBase64String(_encodeImageData);
            }
        }

        [ForeignKey(typeof(SafetyCheckFault))]
        [XmlIgnore]
        public Guid SafetyCheckFaultID 
        { get; set; }

        [XmlIgnore]
        [Ignore]
        // Raw
        public byte[] Bytes
        {
            get { return _bytes; }
            set
            {
                _bytes = value;
                _encodeImageData = Convert.ToBase64String(_bytes);
            }
        }

    }
}
