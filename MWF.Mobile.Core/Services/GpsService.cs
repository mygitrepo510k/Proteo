﻿using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Location;
using System;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Helpers;

namespace MWF.Mobile.Core.Services
{
    public class GpsService : IGpsService, IDisposable
    {

        #region Private Members

        private readonly IMvxLocationWatcher _locationWatcher;
        private bool _disposed = false;
        private MvxGeoLocation _location;

        #endregion

        #region Constructor

        public GpsService(IMvxLocationWatcher locationWatcher)
        {
            _locationWatcher = locationWatcher;
            if (!_locationWatcher.Started)
                _locationWatcher.Start(new MvxLocationOptions(), OnSuccess, OnError);
        }

        #endregion

        #region Public Methods

        public string GetSmpData(ReportReason reportReason)
        {
            var smp = new SMP
            {
                Reason = reportReason,
                Latitude =  Convert.ToDecimal(_location.Coordinates.Latitude),
                Longitude = Convert.ToDecimal(_location.Coordinates.Longitude),
                Speed = Convert.ToInt16(_location.Coordinates.Speed) > (short)2 
                                        ? Convert.ToInt16(_location.Coordinates.Speed) 
                                        : (short)0,
                Quality = Convert.ToInt32(_location.Coordinates.Accuracy),
                LastFixDateTime = _location.Timestamp.DateTime,
                Heading = Convert.ToInt16(_location.Coordinates.Heading),
                ReportDateTime = DateTime.UtcNow
            };

            return smp.ToString();
        }

        #endregion

        #region Private Methods

        private void OnSuccess(MvxGeoLocation location)
        {
            _location = location;
        }
        
        private void OnError(MvxLocationError error)
        {
            Mvx.Error("Location error: {0}", error.Code.ToString());
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_locationWatcher.Started)
                        _locationWatcher.Stop();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
