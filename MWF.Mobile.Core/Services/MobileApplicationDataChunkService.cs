using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Services
{
    public class MobileApplicationDataChunkService : IMobileApplicationDataChunkService
    {

        #region Private Members

        private readonly Repositories.IRepositories _repositories = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;

        #endregion Private Members

        #region Constructors 

        public MobileApplicationDataChunkService(Repositories.IRepositories repositories, IGatewayQueuedService gatewayQueuedService, IGpsService gpsService)
        {
            _repositories = repositories;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
        }

        #endregion Constructors 

        #region Public Members

        public MobileData CurrentMobileData { get; set; }

        #endregion Public Members

        #region Public Methods

        public void Commit()
        {
            var mobileData = CurrentMobileData;
            mobileData.LatestDataChunkSequence++;

            var currentDriver = _repositories.DriverRepository.GetByID(mobileData.DriverId);
            var currentVehicle = _repositories.VehicleRepository.GetByID(mobileData.VehicleId);

            string smp = "";
            MobileApplicationDataChunk dataChunk = new MobileApplicationDataChunk();
            MobileApplicationDataChunkContentActivity dataChunkActivity = new MobileApplicationDataChunkContentActivity();
            MobileApplicationDataChunkContentActivities dataChunkActivities = new MobileApplicationDataChunkContentActivities { MobileApplicationDataChunkContentActivitiesObject = new List<MobileApplicationDataChunkContentActivity>() };
            MobileApplicationDataChunkContentOrder dataChunkOrder = new MobileApplicationDataChunkContentOrder { MobileApplicationDataChunkContentOrderActivities = new List<MobileApplicationDataChunkContentActivities>() };
            MobileApplicationDataChunkCollection dataChunkCollection = new MobileApplicationDataChunkCollection { MobileApplicationDataChunkCollectionObject = new List<MobileApplicationDataChunk>() };


            dataChunkActivity.Activity = 10;
            dataChunkActivity.DriverId = currentDriver.ID;
            dataChunkActivity.EffectiveDate = DateTime.Now;
            dataChunkActivity.EffectiveDate = dataChunkActivity.EffectiveDate.AddMilliseconds(-dataChunkActivity.EffectiveDate.Millisecond);
            dataChunkActivity.MwfVersion = "";
            dataChunkActivity.VehicleRegistration = currentVehicle.Registration;

            dataChunk.EffectiveDate = dataChunkActivity.EffectiveDate;
            dataChunk.ID = Guid.NewGuid();
            dataChunk.MobileApplicationDataID = mobileData.ID;
            dataChunk.SyncState = Enums.SyncState.Add;

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

                    break;

            } 

            dataChunkActivity.Smp = smp;
            dataChunkActivity.Sequence = mobileData.LatestDataChunkSequence;
            dataChunkActivities.MobileApplicationDataChunkContentActivitiesObject.Add(dataChunkActivity);
            dataChunkOrder.MobileApplicationDataChunkContentOrderActivities.Add(dataChunkActivities);

            dataChunk.Data = dataChunkOrder;

            dataChunkCollection.MobileApplicationDataChunkCollectionObject.Add(dataChunk);
            dataChunk.Sequence = mobileData.Sequence;

            _gatewayQueuedService.AddToQueue("fwSyncChunkToServer", dataChunkCollection);

            var instructionToUpdate = _repositories.MobileDataRepository.GetByID(mobileData.ID);
            if (instructionToUpdate != null)
            {
                _repositories.MobileDataRepository.Delete(instructionToUpdate);
            }
            _repositories.MobileDataRepository.Insert(mobileData);

        }

        #endregion Public Methods
    }
}
