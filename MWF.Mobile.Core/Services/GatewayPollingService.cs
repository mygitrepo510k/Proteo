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
        private readonly IStartupService _startupService;
        private readonly IMainService _mainService;

        private readonly string _gatewayDeviceRequestUrl = null;

        private MvxSubscriptionToken _pollTimerMessageToken = null;
        private bool _isSubmitting = false;
        private bool _submitAgainOnCompletion = false;
        private int _dataSpan = -1;


        public GatewayPollingService(IDeviceInfo deviceInfo, IHttpService httpService, IReachability reachability, IRepositories repositories, IMvxMessenger messenger,
            IGatewayService gatewayService, IGatewayQueuedService gatewayQueuedService, IStartupService startupService, IMainService mainService)
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

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";

            _deviceRepository = repositories.DeviceRepository;

        }

        public void StartPollingTimer()
        {
            if (_pollTimerMessageToken == null)
                _pollTimerMessageToken = _messenger.Subscribe<Messages.GatewayPollTimerElapsedMessage>(async m => await PollForInstructionsAsync());

            PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand.Start);
        }

        public void StopPollingTimer()
        {
            if (_pollTimerMessageToken != null)
            {
                _pollTimerMessageToken.Dispose();
                _pollTimerMessageToken = null;
            }

            PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand.Stop);
        }

        public async Task PollForInstructions()
        {
            PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand.Reset);
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

            // Check if we have anything in the response
            if (instructions.Any())
            {
                // We have a response so check what we need to do (Save/Update/Delete)
                foreach (var instruction in instructions)
                {

                    instruction.VehicleId = _startupService.CurrentVehicle.ID;

                    switch (instruction.SyncState)
                    {
                        case SyncState.Add:
                            var instructionToAdd = _repositories.MobileDataRepository.GetByID(instruction.ID);
                            if (instructionToAdd == null)
                                _repositories.MobileDataRepository.Insert(instruction);

                            break;

                        case SyncState.Update:
                            var instructionToUpdate = _repositories.MobileDataRepository.GetByID(instruction.ID);
                            if (instructionToUpdate != null)
                            {
                                var progress = instructionToUpdate.ProgressState;
                                _repositories.MobileDataRepository.Delete(instructionToUpdate);
                                instruction.ProgressState = progress;
                            }
                            _repositories.MobileDataRepository.Insert(instruction);

                            if (!_mainService.OnManifestPage)
                                PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, instruction.ID);
                            break;

                        case SyncState.Delete:
                            var oldInstruction = _repositories.MobileDataRepository.GetByID(instruction.ID);
                            if (oldInstruction != null)
                                _repositories.MobileDataRepository.Delete(oldInstruction);

                            if (!_mainService.OnManifestPage)
                                PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, instruction.ID);
                            break;
                    }
                }
                //Acknowledge that they are on the Device (Not however acknowledged by the driver)
                AcknowledgeInstructions(instructions);

                Mvx.Resolve<ICustomUserInteraction>().PopUpInstructionNotifaction(instructions.ToList(), () => _mainService.SendReadChunk(instructions), "Manifest Update", "Acknowledge");

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

        private void PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand timerCommand)
        {
            _messenger.Publish(new Messages.GatewayPollTimerCommandMessage(this, timerCommand));
        }

        private void PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand command, Guid instructionID)
        {
            _messenger.Publish(new Messages.GatewayInstructionNotificationMessage(this, command, instructionID));
        }
    }
}
