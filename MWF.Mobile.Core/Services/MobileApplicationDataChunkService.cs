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

        private readonly Repositories.IRepositories _repositories = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;

        public MobileApplicationDataChunkService(Repositories.IRepositories repositories, IGatewayQueuedService gatewayQueuedService, IGpsService gpsService)
        {
            _repositories = repositories;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
        }

        public MobileData CurrentMobileData { get; set; }

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
                    dataChunkActivity.Sequence = mobileData.LatestDataChunkSequence;
                    dataChunkActivity.Title = "DRIVE";
                    
                    dataChunk.Sequence = mobileData.Sequence;
                    dataChunk.Title = "DRIVE";
                    break;

                case Enums.InstructionProgress.OnSite:
                    smp = _gpsService.GetSmpData(Enums.ReportReason.OnSite);
                    dataChunkActivity.Sequence = mobileData.LatestDataChunkSequence;
                    dataChunkActivity.Title = "ONSITE";
                    
                    dataChunk.Sequence = mobileData.Sequence;
                    dataChunk.Title = "ONSITE";

                    break;

                case Enums.InstructionProgress.Complete:
                    smp = _gpsService.GetSmpData(Enums.ReportReason.Complete);
                    dataChunkActivity.Sequence = mobileData.LatestDataChunkSequence;
                    dataChunkActivity.Title = "COMPLETE";

                    dataChunk.Sequence = mobileData.Sequence;
                    dataChunk.Title = "COMPLETE";

                    break;

            } 

            dataChunkActivity.Smp = smp;
            dataChunkActivities.MobileApplicationDataChunkContentActivitiesObject.Add(dataChunkActivity);
            dataChunkOrder.MobileApplicationDataChunkContentOrderActivities.Add(dataChunkActivities);

            dataChunk.Data = dataChunkOrder;

            dataChunkCollection.MobileApplicationDataChunkCollectionObject.Add(dataChunk);

            _gatewayQueuedService.AddToQueue("fwSyncChunkToServer", dataChunkCollection);

            var instructionToUpdate = _repositories.MobileDataRepository.GetByID(mobileData.ID);
            if (instructionToUpdate != null)
            {
                _repositories.MobileDataRepository.Delete(instructionToUpdate);
            }
            _repositories.MobileDataRepository.Insert(mobileData);

        }
    }
}
