using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Newtonsoft.Json;

namespace MWF.Mobile.Core.Models
{
    public class Trailer : BaseVehicle
    {

        public Trailer()
            : base()
        {  }

        public Trailer (BaseVehicle baseVehicle)
            : base()
        {
            this.ID = baseVehicle.ID;
            this.Title = baseVehicle.Title;
            this.Registration = baseVehicle.Registration;
            this.SafetyCheckProfileIntLink = baseVehicle.SafetyCheckProfileIntLink;
            this.IsTrailer = IsTrailer;
        }

        public static bool SameAs(Trailer trailer, Instruction.Trailer instructionTrailer)
        {
            if (trailer == null || instructionTrailer == null)
            {
                return (trailer == null && instructionTrailer == null);
            }

            return trailer.Registration == instructionTrailer.TrailerId;
        }

        public static bool SameAs(Trailer trailer1, Trailer trailer2)
        {
            if (trailer1 == null || trailer2 == null)
            {
                return (trailer1 == null && trailer2 == null);
            }

            return trailer1.ID == trailer2.ID;
        }
    }
}
