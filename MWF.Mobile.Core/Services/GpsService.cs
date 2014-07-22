using System.ServiceModel.Channels;
using Cirrious.MvvmCross.Plugins.Location;
using System;

namespace MWF.Mobile.Core.Services
{
    public class GpsService : IGpsService
    {
        private readonly IMvxLocationWatcher _watcher;

        public GpsService(IMvxLocationWatcher watcher)
        {
            _watcher = watcher;
        }

        public string GetSmpData(ReportReason reportReason)
        {
            var location = GetLocation();
            var smp = new SMP
            {
                Reason = reportReason,
                Latitude = 0.00m,
                Longitude = 0.00m,
                Speed = 0,
                Quality = 0,
                LastFixDateTime = DateTime.Now,
                Heading = Convert.ToInt16(0),
                ReportDateTime = DateTime.UtcNow
            };

            return smp.ToString();
        }

        public MvxGeoLocation GetLocation()
        {
            return _watcher.CurrentLocation;        
        }
    }

    public enum ReportReason
    {
        RPTRSN_TIMED = 0x01,
        RPTRSN_DIST = 0x02,
        RPTRSN_SLEEP = 0x03,
        RPTRSN_REQ = 0x04,
        RPTRSN_IDLE = 0x05,
        RPTRSN_IGNON = 0x06,
        RPTRSN_IGNOFF = 0x07,
        RPTRSN_LOGON = 0x08,
        RPTRSN_LOGOFF = 0x09,
        RPTRSN_POWOFF = 0x11,
        RPTRSN_POWON = 0x12,
        RPTRSN_EXT = 0x20,
        RPTRSN_DOCK = 0x13,
        RPTRSN_UNDOCK = 0x14,
        SMPREASON_TimedAndGritterOff = 0x15,
        SMPREASON_TimedAndGritterOn = 0x16,
        SMPREASON_POWER_LOSS = 0x17,
        SMPREASON_POWER_RESTORE = 0x18,
        SMPREASON_DEVICE_DOCKED = 0x19,
        SMPREASON_DEVICE_UNDOCKED = 0x20,
        SMPREASON_OVERREVVING = 0x21,
        SMPREASON_HARSHBRAKING = 0x22,
        SMPREASON_SPEEDING = 0x23,
        SMPREASON_HEADINGCHANGE = 0x24,
        SMPREASON_IDLESTART = 0x25,
        SMPREASON_DATARECIEVED = 0x32,
        SMPREASON_SAFETYCHECKREPORT = 0x133,
        SMPREASON_VEHICLEACCIDENT = 0x134,
        SMPREASON_BEGIN = 0x170,
        SMPREASON_DRIVE = 0x171,
        SMPREASON_ONSITE = 0x172,
        SMPREASON_CANCEL = 0x173,
        SMPREASON_SUSPEND = 0x174,
        SMPREASON_RESUME = 0x175,
        SMPREASON_COMPLETE = 0x176,
        SMPREASON_PROBLEM = 0x177,
        SMPREASON_MANIFESTCOMPLETE = 0x178,
        SMPREASON_OFFSITE = 0x179,
        SMPREASON_COMMENT = 0x180,
        SMPREASON_END = 0x181,
        SMPREASON_ACCEPT = 0x182,
        SMPREASON_WATERADDED = 0x183,
        SMPREASON_LOADED = 0x184,
        SMPREASON_TIP = 0x185,
        SMPREASON_TRAILER = 0x186,
        SMPREASON_UNKNOWN = 0x999,
    }

    public class SMP
    {
        public int MTC { get; set; }
        public int URI { get; set; }
        public int ID { get; set; }
        public ReportReason Reason { get; set; }
        public DateTime ReportDateTime { get; set; }
        public DateTime LastFixDateTime { get; set; }
        public int Quality { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public short Heading { get; set; }
        public short Speed { get; set; }

        public override string ToString()
        {
            // <83042209E7F22E0CB787253325195001491164090038>
            //83 04 2209E7F2 2E0CB787 2 5332519500149116 409 003 8

            string SMPMessage = string.Empty;
            try
            {
                SMPMessage = "83"; // MTC 
                SMPMessage += ((int)this.Reason).ToString("X2"); // ReasonCode
                SMPMessage += DateTimeToSMP(this.ReportDateTime).ToString("X8"); // Last Resp
                SMPMessage += DateTimeToSMP(this.LastFixDateTime).ToString("X8"); // Last Pos
                SMPMessage += string.Format("{0:X1}", (byte)Math.Min(this.Quality, 9)); // Quality
                SMPMessage += ConvertLatLonToSMP(this.Latitude, this.Longitude); // Signs Lat Lon 
                SMPMessage += string.Format("{0:000}", this.Heading); // Heading
                SMPMessage += string.Format("{0:000}", this.Speed); // Speed


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return SMPMessage;
        }


        // this is the number of seconds since 06 Jan 1980 
        private Int32 DateTimeToSMP(DateTime rtc)
        {
            DateTime baseDate = new DateTime(1980, 01, 06, 00, 00, 00);

            Int32 seconds = (Int32)(rtc - baseDate).TotalSeconds;

            return seconds;
        }

        private string ConvertLatLonToSMP(decimal lat, decimal lon)
        {
            string retVal = string.Empty;
            try
            {
                Int32 sign = ((lat >= 0) ? 2 : 0) | ((lon >= 0) ? 1 : 0);

                retVal = sign.ToString();
                retVal += ConvertToRealNumber((double)lat, 6).ToString("00000000");
                retVal += ConvertToRealNumber((double)lon, 6).ToString("000000000");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retVal;

        }

        private Int32 ConvertToRealNumber(double value, int decimalPlaces)
        {
            double retVal = value * (System.Math.Pow(10, decimalPlaces));
            retVal = Math.Floor(retVal);

            if (retVal < 0)
                retVal = retVal * -1;

            return (Int32)retVal;
        }
    }
}
