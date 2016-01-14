using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Portable;
using MWF.Mobile.Core.Presentation;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Utilities;
using MWF.Mobile.Core.ViewModels;

namespace MWF.Mobile.Core.Services
{

    public class GatewayPollingService : IGatewayPollingService
    {

        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IHttpService _httpService = null;
        private readonly IReachability _reachability = null;
        private readonly IRepositories _repositories;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ICustomPresenter _customPresenter;

        private readonly IMvxMessenger _messenger = null;
        private readonly IGatewayService _gatewayService = null;
        private readonly IGatewayQueuedService _gatewayQueuedService = null;
        private readonly ILoggingService _loggingService = null;
        private Timer _timer;
        private readonly IInfoService _infoService;
        IDataChunkService _dataChunkService;
        private readonly AsyncLock _lock = new AsyncLock();

        private int? _dataRetention = null;
        private int? _dataSpan = null;

        public GatewayPollingService(
            IDeviceInfo deviceInfo,
            IHttpService httpService,
            IReachability reachability,
            IRepositories repositories,
            IMvxMessenger messenger,
            IGatewayService gatewayService,
            IGatewayQueuedService gatewayQueuedService,
            IInfoService infoService,
            IDataChunkService dataChunkService,
            ICustomPresenter customPresenter,
            ILoggingService loggingService
            )
        {
            _deviceInfo = deviceInfo;
            _httpService = httpService;
            _reachability = reachability;
            _repositories = repositories;
            _messenger = messenger;
            _gatewayService = gatewayService;
            _gatewayQueuedService = gatewayQueuedService;
            _infoService = infoService;
            _dataChunkService = dataChunkService;
            _customPresenter = customPresenter;

            _deviceRepository = repositories.DeviceRepository;
            _loggingService = loggingService;
        }

        public void StartPollingTimer()
        {
            if (_timer == null)
            {
                _timer = new Timer(async state =>
                {
                    await this.PollForInstructionsAsync();
                    _timer.Reset();
                },
                null,
                60000);
            }
        }

        public void StopPollingTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public async Task PollForInstructionsAsync()
        {
			try
            {
                if (!_reachability.IsConnected())
                    return;

                using (await _lock.LockAsync())
                {
                    var data = await _repositories.ApplicationRepository.GetAllAsync();
                    var applicationProfile = data.First();
                    _dataRetention = applicationProfile.DataRetention;
                    _dataSpan = applicationProfile.DataSpan;
                }

                Mvx.Trace("Begin Polling For Instructions");
				string exceptionMsg = null;

                try
                {
                    if (!_dataRetention.HasValue || !_dataSpan.HasValue)
                    {
                        var applicationProfile = await _repositories.ApplicationRepository.GetAsync();
                        _dataRetention = applicationProfile.DataRetention;
                        _dataSpan = applicationProfile.DataSpan;
                    }

                    // Call to BlueSphere to check for instructions
                    var instructions = await _gatewayService.GetDriverInstructionsAsync(
                        _infoService.CurrentVehicle.Registration,
                        _infoService.LoggedInDriver.ID,
                        DateTime.Today.AddDays(-_dataRetention.Value),
                        DateTime.Today.AddDays(_dataSpan.Value));

                    // Check if we have anything in the response
                    if (!instructions.Any())
                        Mvx.Trace("No instructions were available.");
                    else
                    {
						Mvx.Trace(string.Format("Successfully pulled {0} instructions.", instructions.Count()));

                        var currentViewModel = _customPresenter.CurrentFragmentViewModel as BaseFragmentViewModel;
                        var manifestInstructionVMsForNotification = new List<ManifestInstructionViewModel>(instructions.Count());

                        // We have a response so check what we need to do (Save/Update/Delete)
                        foreach (var instruction in instructions)
                        {
                            var notifyInstruction = false;

                            Mvx.Trace("started processing instruction." + instruction.ID);

                            instruction.VehicleId = _infoService.CurrentVehicle.ID;

                            switch (instruction.SyncState)
                            {
                                case SyncState.Add:

                                    Mvx.Trace("started adding instruction." + instruction.ID);
                                    var instructionToAdd = await _repositories.MobileDataRepository.GetByIDAsync(instruction.ID);

                                    if (instructionToAdd == null)
                                    {
                                        try
                                        {
                                            await _repositories.MobileDataRepository.InsertAsync(instruction);
                                        }
                                        catch (Exception ex)
                                        {
                                            MvxTrace.Error("\"{0}\" in {1}.{2}\n{3}", ex.Message, "MobileDataRepository", "InsertAsync", ex.StackTrace);
                                            throw;
                                        }

                                        notifyInstruction = true;
                                    }

                                    Mvx.Trace("completed adding instruction." + instruction.ID);
                                    PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Add, instruction.ID);
                                    break;

                                case SyncState.Update:
                                    Mvx.Trace("started updating instruction." + instruction.ID);
                                    var instructionToUpdate = await _repositories.MobileDataRepository.GetByIDAsync(instruction.ID);

                                    if (instructionToUpdate != null)
                                    {
                                        var progress = instructionToUpdate.ProgressState;
                                        await _repositories.MobileDataRepository.DeleteAsync(instructionToUpdate);
                                        instruction.ProgressState = progress;
                                        instruction.LatestDataChunkSequence = instructionToUpdate.LatestDataChunkSequence;
                                    }

                                    try
                                    {
                                        await _repositories.MobileDataRepository.InsertAsync(instruction);
                                    }
                                    catch (Exception ex)
                                    {
                                        MvxTrace.Error("\"{0}\" in {1}.{2}\n{3}", ex.Message, "MobileDataRepository", "InsertAsync", ex.StackTrace);
                                        throw;
                                    }

                                    notifyInstruction = true;
                                    Mvx.Trace("completed updating instruction." + instruction.ID);
                                    PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Update, instruction.ID);
                                    break;

                                case SyncState.Delete:
                                    Mvx.Trace("started deleting instruction." + instruction.ID);
                                    var oldInstruction = await _repositories.MobileDataRepository.GetByIDAsync(instruction.ID);

                                    if (oldInstruction != null)
                                    {
                                        await _repositories.MobileDataRepository.DeleteAsync(oldInstruction);

                                        if (oldInstruction.ProgressState != InstructionProgress.Complete)
                                            notifyInstruction = true;
                                    }

                                    Mvx.Trace("completed deleting instruction." + instruction.ID);
                                    PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand.Delete, instruction.ID);
                                    break;
                            }

                            if (notifyInstruction)
                                manifestInstructionVMsForNotification.Add(new ManifestInstructionViewModel(currentViewModel, instruction));
                        }

                        Mvx.Trace("Successfully inserted instructions into Repository.");

                        //Acknowledge that they are on the Device (Not however acknowledged by the driver)
                        await AcknowledgeInstructionsAsync(instructions);

                        Mvx.Trace("Successfully sent device acknowledgement.");

                        if (manifestInstructionVMsForNotification.Any())
                        {
                            Mvx.Resolve<ICustomUserInteraction>().PopUpInstructionNotification(
                                manifestInstructionVMsForNotification,
                                done: async notifiedInstructionVMs => await this.SendReadChunksAsync(notifiedInstructionVMs),
                                title: "Manifest Update",
                                okButton: "Acknowledge");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // catch and log the error, but this will not acknowledge the message so we can try again
					exceptionMsg = ex.Message;
                }

				if (exceptionMsg != null)
					await _loggingService.LogEventAsync("Gateway Polling Processing Failed", LogType.Error, exceptionMsg);
			}
            catch (Exception ex)
            {
                // if there is an error here, then just continue as this is probably related to a connection issue
                Mvx.Trace("failed to poll for instructions", ex);
            }
        }

        private Task SendReadChunksAsync(IEnumerable<ManifestInstructionViewModel> manifestInstructionViewModels)
        {
            var instructions = manifestInstructionViewModels.Select(i => i.MobileData).ToList();
            return _dataChunkService.SendReadChunkAsync(instructions, _infoService.LoggedInDriver, _infoService.CurrentVehicle);
        }

        private Task AcknowledgeInstructionsAsync(IEnumerable<MobileData> instructions)
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

            return _gatewayQueuedService.AddToQueueAsync(syncAckActions);
        }

        private void PublishInstructionNotification(Messages.GatewayInstructionNotificationMessage.NotificationCommand command, Guid instructionID)
        {
            _messenger.Publish(new Messages.GatewayInstructionNotificationMessage(this, command, instructionID));
        }
    }
}
