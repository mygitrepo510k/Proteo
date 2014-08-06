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

namespace MWF.Mobile.Core.Services
{

    public class GatewayPollingService : IGatewayPollingService
    {

        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IHttpService _httpService = null;
        private readonly Portable.IReachability _reachability = null;
        private readonly IRepositories _repositories;
        private readonly IDeviceRepository _deviceRepository;

        private readonly IMvxMessenger _messenger = null;
        private readonly IGatewayService _gatewayService = null;
        private readonly IStartupService _startupService;

        private readonly string _gatewayDeviceRequestUrl = null;

        private MvxSubscriptionToken _pollTimerMessageToken = null;
        private bool _isSubmitting = false;
        private bool _submitAgainOnCompletion = false;

        public GatewayPollingService(IDeviceInfo deviceInfo, IHttpService httpService, Portable.IReachability reachability, IRepositories repositories, IMvxMessenger messenger, IGatewayService gatewayService, IStartupService startupService)
        {
            _deviceInfo = deviceInfo;
            _httpService = httpService;
            _reachability = reachability;
            _repositories = repositories;
            _messenger = messenger;
            _gatewayService = gatewayService;
            _startupService = startupService;

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
            _pollTimerMessageToken.Dispose();
            _pollTimerMessageToken = null;

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
                                                                           DateTime.Now);
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
                    switch (instruction.SyncState)
                    {
                        case SyncState.Add:
                             var instructionToAdd = _repositories.MobileDataRepository.GetByID(instruction.ID);
                             if (instructionToAdd == null)
                             {
                                 _repositories.MobileDataRepository.Insert(instruction);
                             }
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
                            break;
                        case SyncState.Delete:
                            var oldInstruction = _repositories.MobileDataRepository.GetByID(instruction.ID);
                            if (oldInstruction != null)
                                _repositories.MobileDataRepository.Delete(oldInstruction);
                            break;
                    }
                }
            }
        }

        private void PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand timerCommand)
        {
            _messenger.Publish(new Messages.GatewayPollTimerCommandMessage(this, timerCommand));
        }
    }
}
