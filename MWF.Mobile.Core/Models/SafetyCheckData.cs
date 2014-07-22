﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;
using MWF.Mobile.Core.Models.Attributes;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Models
{

    // Model class which holds represents the results of a safety check performed by a driver on a vehicle
    public class SafetyCheckData: IBlueSphereEntity
    {

        public SafetyCheckData()
        {
            this.ID = Guid.NewGuid();
        }

        [Unique]
        [PrimaryKey]
        [XmlAttribute("id")]
        public Guid ID { get; set; }

        [XmlAttribute("driverid")]
        public Guid DriverID { get; set; }

        [XmlAttribute("vehicleid")]
        public Guid VehicleID { get; set; }

        [XmlAttribute("vehicleregistration")]
        public string VehicleRegistration { get; set; }

        [XmlAttribute("driver")]
        public string DriverTitle { get; set; }

        [XmlAttribute("effectivedate")]
        public DateTime EffectiveDate { get; set; }

        [XmlAttribute("mileage")]
        public int Mileage { get; set; }

        [XmlAttribute("smp")]
        public string SMP { get; set; }

        [ChildRelationship(typeof(Signature), RelationshipCardinality.OneToOne)]
        [XmlElement("signature")]
        public Signature Signature { get; set; }

        [ChildRelationship(typeof(SafetyCheckFault))]
        [XmlArray("faults")]
        public List<SafetyCheckFault> Faults { get; set; }

        [XmlAttribute("intlink")]
        public int ProfileIntLink { get; set; }

    }

}
