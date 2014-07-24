using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Core.Models
{
    // Model class which represents a fault for a specific safety check item 9SafetyCheckFaultType)
    public class SafetyCheckFault: IBlueSphereEntity
    {

        public SafetyCheckFault()
        {
            Images = new List<Image>();
        }

        [Unique]
        [PrimaryKey]
        [JsonProperty("@id")]
        public Guid ID { get; set; }

        [JsonProperty("@title")]
        public string Title { get; set; }

        [JsonProperty("@faultid")]
        public Guid FaultTypeID { get; set; }

        [JsonProperty("@reference")]
        public string FaultTypeReference { get; set; }

        [JsonProperty("@comment")]
        public string Comment { get; set; }

        [JsonProperty("@discrete")]
        public bool IsDiscretionaryPass { get; set; }

        [ChildRelationship(typeof(Image))]
        [JsonProperty("image")]
        [JsonConverter(typeof(JsonWrappedListConverter<Image>))]
        public List<Image> Images { get; set; }

        [ForeignKey(typeof(SafetyCheckData))]
        public Guid SafetyCheckDataID { get; set; }

        #region Public Methods

        public SafetyCheckFault Clone()
        {
            //simple memberwise clone deal with all value types
            SafetyCheckFault clone = this.MemberwiseClone() as SafetyCheckFault;
            
            // deep copy of images

            clone.Images = new List<Image>();

            foreach (Image image in this.Images)
            {
                clone.Images.Add(image);
            }

            return clone;
        }

        //Sets the safety check fault with member values from another safety check fault
        public void ValuesFrom(SafetyCheckFault sourceFault)
        {

            this.ID = sourceFault.ID;
            this.FaultTypeID = sourceFault.FaultTypeID;
            this.Comment = sourceFault.Comment;
            this.IsDiscretionaryPass = sourceFault.IsDiscretionaryPass;
            this.FaultTypeReference = sourceFault.FaultTypeReference;
            this.SafetyCheckDataID = sourceFault.SafetyCheckDataID;

            this.Images.Clear();

            foreach (Image image in sourceFault.Images)
            {
                this.Images.Add(image);
            }

        }

        #endregion

    }
}
