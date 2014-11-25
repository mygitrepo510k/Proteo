using Cirrious.MvvmCross.ViewModels;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Services
{
    public class MainService 
        : MvxNavigatingObject, IMainService
    {

        #region Private Members

        private readonly Repositories.IRepositories _repositories = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;

        #endregion Private Members

        #region Constructors 

        public MainService(Repositories.IRepositories repositories, IGatewayQueuedService gatewayQueuedService, IGpsService gpsService)
        {
            _repositories = repositories;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
        }

        #endregion Constructors 

        #region Public Members

        public Driver CurrentDriver { get; set; }
        public Vehicle CurrentVehicle { get; set; }

        public MobileData CurrentMobileData { get; set; }
        public MobileApplicationDataChunkContentActivity CurrentDataChunkActivity { get; set; }

        public bool OnManifestPage { get; set; }

        #endregion Public Members

        #region Public Methods

        public void SendPhotoAndComment(string comment, List<Image> photos)
        {
            UploadCameraImageObject imageUpload = new UploadCameraImageObject();
            imageUpload.Smp = _gpsService.GetSmpData(Enums.ReportReason.Comment);
            imageUpload.ID = Guid.NewGuid();
            imageUpload.DriverTitle = CurrentDriver.DisplayName;
            imageUpload.DriverId = CurrentDriver.ID;
            imageUpload.Pictures = photos;
            imageUpload.Comment = comment;
            imageUpload.DateTimeOfUpload = DateTime.Now;

            if (!OnManifestPage)
                imageUpload.MobileApplicationID = CurrentMobileData.ID;

            _gatewayQueuedService.AddToQueue("fwSyncPhotos", imageUpload);
        }

        public void SendDataChunk(bool updateQuantity = false)
        {
            var mobileData = CurrentMobileData;
            mobileData.LatestDataChunkSequence++;

            bool deleteMobileData = false;
            string smp = "";
            MobileApplicationDataChunk dataChunk = new MobileApplicationDataChunk();
            MobileApplicationDataChunkContentActivity dataChunkActivity = CurrentDataChunkActivity;
            MobileApplicationDataChunkContentActivities dataChunkActivities = new MobileApplicationDataChunkContentActivities { MobileApplicationDataChunkContentActivitiesObject = new List<MobileApplicationDataChunkContentActivity>() };
            MobileApplicationDataChunkContentOrder dataChunkOrder = new MobileApplicationDataChunkContentOrder { MobileApplicationDataChunkContentOrderActivities = new List<MobileApplicationDataChunkContentActivities>() };
            MobileApplicationDataChunkCollection dataChunkCollection = new MobileApplicationDataChunkCollection { MobileApplicationDataChunkCollectionObject = new List<MobileApplicationDataChunk>() };

            if (dataChunkActivity == null)
            {
                dataChunkActivity = new MobileApplicationDataChunkContentActivity();
                CurrentDataChunkActivity = dataChunkActivity;
            }
                dataChunkActivity.Activity = 10;
                dataChunkActivity.DriverId = CurrentDriver.ID;
                dataChunkActivity.EffectiveDate = DateTime.Now;
                dataChunkActivity.EffectiveDate = dataChunkActivity.EffectiveDate.AddMilliseconds(-dataChunkActivity.EffectiveDate.Millisecond);
                dataChunkActivity.MwfVersion = "";
                dataChunkActivity.VehicleRegistration = CurrentVehicle.Registration;

                dataChunk.EffectiveDate = dataChunkActivity.EffectiveDate;
                dataChunk.ID = Guid.NewGuid();
                dataChunk.MobileApplicationDataID = mobileData.ID;
                dataChunk.SyncState = Enums.SyncState.Add;

                if (updateQuantity)
                {
                    smp = _gpsService.GetSmpData(Enums.ReportReason.ActiveReport);
                    dataChunkActivity.Title = "REVISED QUANTITY";

                    dataChunk.Title = "REVISED QUANTITY";
                }
                else
                {
                    switch (mobileData.ProgressState)
                    {
                        case Enums.InstructionProgress.Driving:
                            smp = _gpsService.GetSmpData(Enums.ReportReason.Drive);
                            dataChunkActivity.Title = "DRIVE";

                            dataChunk.Title = "DRIVE";
                            break;

                        case Enums.InstructionProgress.OnSite:
                            smp = _gpsService.GetSmpData(Enums.ReportReason.OnSite);
                            dataChunkActivity.Title = "ONSITE";

                            dataChunk.Title = "ONSITE";

                            break;

                        case Enums.InstructionProgress.Complete:
                            smp = _gpsService.GetSmpData(Enums.ReportReason.Complete);
                            dataChunkActivity.Title = "COMPLETE";

                            dataChunk.Title = "COMPLETE";

                            deleteMobileData = true;

                            break;
                    }
                }

            dataChunkActivity.Smp = smp;
            dataChunkActivity.Sequence = mobileData.LatestDataChunkSequence;
            dataChunkActivities.MobileApplicationDataChunkContentActivitiesObject.Add(dataChunkActivity);
            dataChunkOrder.MobileApplicationDataChunkContentOrderActivities.Add(dataChunkActivities);

            dataChunk.Data = dataChunkOrder;

            dataChunkCollection.MobileApplicationDataChunkCollectionObject.Add(dataChunk);
            dataChunk.Sequence = mobileData.Sequence;

            CurrentDataChunkActivity = dataChunkActivity;

            _gatewayQueuedService.AddToQueue("fwSyncChunkToServer", dataChunkCollection);

            if (deleteMobileData)
            {
                var oldMobileData = _repositories.MobileDataRepository.GetByID(mobileData.ID);
                if (oldMobileData != null)
                    _repositories.MobileDataRepository.Delete(oldMobileData);
            }
            else
            {
                var mobileDataToUpdate = _repositories.MobileDataRepository.GetByID(mobileData.ID);
                if (mobileDataToUpdate != null)
                {
                    _repositories.MobileDataRepository.Delete(mobileDataToUpdate);
                }
                _repositories.MobileDataRepository.Insert(mobileData);
            }
        }

        #endregion Public Methods
    }
}
