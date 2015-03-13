﻿using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public class DataChunkService : IDataChunkService
    {

        #region Private Members

        private readonly Repositories.IRepositories _repositories = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly IGpsService _gpsService = null;
        private readonly ILoggingService _loggingService = null;
        private IMainService _mainService = null;

        #endregion Private Members

        #region Constructors

        public DataChunkService(
            Repositories.IRepositories repositories,
            IGatewayQueuedService gatewayQueuedService,
            IGpsService gpsService,
            ILoggingService loggingService,
            IMainService mainService)
        {
            _repositories = repositories;
            _gatewayQueuedService = gatewayQueuedService;
            _gpsService = gpsService;
            _loggingService = loggingService;
            _mainService = mainService;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// This method sends the instructions that have been acknowledged by the driver.
        /// This is in the form of a 'Read' Chunk
        /// </summary>
        /// <param name="instructions">The instruction that have been acknowledged</param>
        public void SendReadChunk(IEnumerable<MobileData> instructions, Driver currentDriver, Vehicle currentVehicle)
        {
            //The data chunk to be sent.
            MobileApplicationDataChunkCollection dataChunkCollection = new MobileApplicationDataChunkCollection { MobileApplicationDataChunkCollectionObject = new List<MobileApplicationDataChunk>() };

            foreach (var instruction in instructions)
            {
                MobileApplicationDataChunk dataChunk = new MobileApplicationDataChunk();
                MobileApplicationDataChunkContentActivity dataChunkActivity = new MobileApplicationDataChunkContentActivity();

                //These variables make up the Data variable in the MobileApplicationDataChunk object.
                MobileApplicationDataChunkContentActivities dataChunkActivities = new MobileApplicationDataChunkContentActivities { MobileApplicationDataChunkContentActivitiesObject = new List<MobileApplicationDataChunkContentActivity>() };
                MobileApplicationDataChunkContentOrder dataChunkOrder = new MobileApplicationDataChunkContentOrder { MobileApplicationDataChunkContentOrderActivities = new List<MobileApplicationDataChunkContentActivities>() };

                instruction.LatestDataChunkSequence++;

                dataChunkActivity.Activity = 10;
                dataChunkActivity.DriverId = currentDriver.ID;
                dataChunkActivity.EffectiveDate = DateTime.Now;
                dataChunkActivity.EffectiveDate = dataChunkActivity.EffectiveDate.AddMilliseconds(-dataChunkActivity.EffectiveDate.Millisecond);
                dataChunkActivity.MwfVersion = "";
                dataChunkActivity.VehicleRegistration = currentVehicle.Registration;
                dataChunkActivity.Smp = _gpsService.GetSmpData(Enums.ReportReason.Begin);
                dataChunkActivity.Title = "READ";
                dataChunkActivity.Sequence = instruction.LatestDataChunkSequence;

                dataChunkActivities.MobileApplicationDataChunkContentActivitiesObject.Add(dataChunkActivity);
                dataChunkOrder.MobileApplicationDataChunkContentOrderActivities.Add(dataChunkActivities);

                dataChunk.EffectiveDate = dataChunkActivity.EffectiveDate;
                dataChunk.ID = Guid.NewGuid();
                dataChunk.MobileApplicationDataID = instruction.ID;
                dataChunk.SyncState = Enums.SyncState.Add;
                dataChunk.Title = "READ";
                dataChunk.Data = dataChunkOrder;
                dataChunk.Sequence = instruction.Sequence;

                dataChunkCollection.MobileApplicationDataChunkCollectionObject.Add(dataChunk);

                if (instruction.SyncState != Enums.SyncState.Delete)
                {

                    var mobileDataToUpdate = _repositories.MobileDataRepository.GetByID(instruction.ID);
                    if (mobileDataToUpdate != null)
                    {
                        _repositories.MobileDataRepository.Delete(mobileDataToUpdate);
                    }
                    _repositories.MobileDataRepository.Insert(instruction);
                }

            }

            _gatewayQueuedService.AddToQueue("fwSyncChunkToServer", dataChunkCollection);

        }

        /// <summary>
        /// This method sends the MobileApplicationDataChunk up to bluesphere,
        /// this is called for when the instruction goes into Drive, OnSite and is Completed
        /// </summary>
        /// <param name="updateQuantity"></param>
        public void SendDataChunk(MobileData currentMobileData, Driver currentDriver, Vehicle currentVehicle, bool updateQuantity = false)
        {
            var mobileData = currentMobileData;
            mobileData.LatestDataChunkSequence++;

            bool deleteMobileData = false;
            string smp = "";

            //These variables make up the Data variable in the MobileApplicationDataChunk object.
            MobileApplicationDataChunkContentActivity dataChunkActivity = _mainService.CurrentDataChunkActivity;
            MobileApplicationDataChunkContentActivities dataChunkActivities = new MobileApplicationDataChunkContentActivities { MobileApplicationDataChunkContentActivitiesObject = new List<MobileApplicationDataChunkContentActivity>() };
            MobileApplicationDataChunkContentOrder dataChunkOrder = new MobileApplicationDataChunkContentOrder { MobileApplicationDataChunkContentOrderActivities = new List<MobileApplicationDataChunkContentActivities>() };

            //The data chunk to be sent.
            MobileApplicationDataChunk dataChunk = new MobileApplicationDataChunk();
            MobileApplicationDataChunkCollection dataChunkCollection = new MobileApplicationDataChunkCollection { MobileApplicationDataChunkCollectionObject = new List<MobileApplicationDataChunk>() };

            if (dataChunkActivity == null)
            {
                dataChunkActivity = new MobileApplicationDataChunkContentActivity();
                _mainService.CurrentDataChunkActivity = dataChunkActivity;
            }

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
            dataChunk.Sequence = mobileData.Sequence;

            dataChunkCollection.MobileApplicationDataChunkCollectionObject.Add(dataChunk);

            _mainService.CurrentDataChunkActivity = dataChunkActivity;

            _gatewayQueuedService.AddToQueue("fwSyncChunkToServer", dataChunkCollection);

            //Delete the instruction from the repository if its completed else just update it.
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