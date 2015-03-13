using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using MWF.Mobile.Core.Portable;
using Chance.MvvmCross.Plugins.UserInteraction;
using MWF.Mobile.Core.Utilities;

namespace MWF.Mobile.Core.Services
{

    public class GatewayPollingService : IGatewayPollingService
    {

        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IHttpService _httpService = null;
        private readonly IReachability _reachability = null;
        private readonly IRepositories _repositories;
        private readonly IDeviceRepository _deviceRepository;

        private readonly IMvxMessenger _messenger = null;
        private readonly IGatewayService _gatewayService = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private Timer _timer;
        private readonly IStartupService _startupService;
        private readonly IMainService _mainService;
        IDataChunkService _dataChunkService;

        private readonly string _gatewayDeviceRequestUrl = null;

        private MvxSubscriptionToken _pollTimerMessageToken = null;
        private bool _isSubmitting = false;
        private bool _submitAgainOnCompletion = false;
        private int _dataSpan = -1;


        public GatewayPollingService(
            IDeviceInfo deviceInfo,
            IHttpService httpService,
            IReachability reachability,
            IRepositories repositories,
            IMvxMessenger messenger,
            IGatewayService gatewayService,
            IGatewayQueuedService gatewayQueuedService,
            IStartupService startupService,
            IMainService mainService,
            IDataChunkService dataChunkService)
        {
            _deviceInfo = deviceInfo;
            _httpService = httpService;
            _reachability = reachability;
            _repositories = repositories;
            _messenger = messenger;
            _gatewayService = gatewayService;
            _gatewayQueuedService = gatewayQueuedService;
            _startupService = startupService;
            _mainService = mainService;
            _dataChunkService = dataChunkService;


            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";

            _deviceRepository = repositories.DeviceRepository;

        }

        private void TimerCallback(object state)
        {
            Task.Run(async () => await PollForInstructionsAsync());

            _timer.Reset();
        }

        public void StartPollingTimer()
        {
            if (_timer == null)
                _timer = new Timer(TimerCallback, null, 30000);
        }

        public void StopPollingTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public async Task PollForInstructions()
        {
            await PollForInstructionsAsync();
        }

        private async Task PollForInstructionsAsync()
        {
            Mvx.Trace("Begin Polling For Instructions");

            if (!_reachability.IsConnected())
                return;

            IEnumerable<MobileData> instructions = new List<MobileData>();

            try
            {
                // Call to BlueSphere to check for instructions
                instructions = await _gatewayService.GetDriverInstructions(_startupService.CurrentVehicle.Registration,
                                                                           _startupService.LoggedInDriver.ID,
                                                                           DateTime.Now,
                                                                           DateTime.Now.AddDays(DataSpan));

            }
            catch (Exception ex)
            {
                Mvx.Trace("Error deserialising instructions: " + ex.Message);
            }


            Mvx.Trace("Successfully pulled instructions.");

            // Check if we have anything in the response
            if (instructions.Any())
            {
                // We have a response so check what we need to do (Save/Update/Delete)
                foreach (var instruction in instructions)
                {
                    Mvx.Trace("started processing instruction." + instruction.ID);

                    try
                    {
                        instruction.VehicleId = _startupService.CurrentVehicle.ID;

                        switch (instruction.SyncState)
                        {
                            case SyncState.Add:

                                Mvx.Trace("started adding instruction." + instruction.ID);
                                var instructionToAdd = _repositories.MobileDataRepository.GetByID(instruction.ID);
                                if (instructionToAdd == null)
                                    _repositories.MobileDataRepository.Insert(instruction);
                                Mvx.Trace("completed adding instruction." + instruction.ID);

                                PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Add, instruction.ID);
                                break;

                            case SyncState.Update:
                                Mvx.Trace("started updating instruction." + instruction.ID);
                                var instructionToUpdate = _repositories.MobileDataRepository.GetByID(instruction.ID);
                                if (instructionToUpdate != null)
                                {
                                    var progress = instructionToUpdate.ProgressState;
                                    _repositories.MobileDataRepository.Delete(instructionToUpdate);
                                    instruction.ProgressState = progress;
                                }

                                _repositories.MobileDataRepository.Insert(instruction);
                                Mvx.Trace("completed updating instruction." + instruction.ID);

                                PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, instruction.ID);
                                break;

                            case SyncState.Delete:
                                Mvx.Trace("started deleting instruction." + instruction.ID);
                                var oldInstruction = _repositories.MobileDataRepository.GetByID(instruction.ID);
                                if (oldInstruction != null)
                                    _repositories.MobileDataRepository.Delete(oldInstruction);
                                Mvx.Trace("completed deleting instruction." + instruction.ID);

                                PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, instruction.ID);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Mvx.Trace("Error modifying instructions in Repository." + ex.Message);
                        Mvx.Resolve<IUserInteraction>().Alert("Error modifying instructions in Repository");
                    }
                }

                Mvx.Trace("Successfully inserted instructions into Repository.");

                try
                {
                    //Acknowledge that they are on the Device (Not however acknowledged by the driver)
                    AcknowledgeInstructions(instructions);
                }
                catch (Exception ex)
                {
                    Mvx.Trace("Error acknowledging instructions." + ex.Message);
                }

                Mvx.Trace("Successfully sent device acknowledgement.");

                Mvx.Resolve<ICustomUserInteraction>().PopUpInstructionNotifaction(instructions.ToList(), () => 
                    _dataChunkService.SendReadChunk(instructions, _mainService.CurrentDriver, _mainService.CurrentVehicle), "Manifest Update", "Acknowledge");

            }
        }

        private void AcknowledgeInstructions(IEnumerable<MobileData> instructions)
        {
            //Sends acknowledgement to bluesphere that the device has received the new instructions
            var syncAckActions = instructions.Select(i => new Models.GatewayServiceRequest.Action<Models.SyncAck>
            {
                Command = "fwSyncAck",
                Parameters = new[]
                    {
                        new Parameter { Name = "MobileApplicationDataID", Value = i.ID.ToString() },
                        new Parameter { Name = "SyncAck", Value = "1" },
                    }
            });

            _gatewayQueuedService.AddToQueue(syncAckActions);
        }

        public int DataSpan
        {

            get
            {
                return (_dataSpan < 0) ? _dataSpan = _repositories.ApplicationRepository.GetAll().First().DataSpan : _dataSpan;
            }

        }

        private void PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand command, Guid instructionID)
        {
            _messenger.Publish(new Messages.GatewayInstructionNotificationMessage(this, command, instructionID));
        }
    }
}
