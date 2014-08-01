using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using MWF.Mobile.Core.Enums;
using MWF.Mobile.Core.Models.GatewayServiceRequest;
using Newtonsoft.Json;
using MWF.Mobile.Core.Repositories;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Services
{

    public class GatewayPollingService
        : IGatewayPollingService
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

        public void PollForInstructions()
        {
            // Always attempt to sync with the MWF Mobile Gateway service whenever items are added to the queue (providing the GatewayQueueTimerService has been started)
            PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand.Trigger);
        }

        private async Task PollForInstructionsAsync()
        {
            Mvx.Trace("Begin Polling For Instructions");

            if (!_reachability.IsConnected())
                return;

            // Call to BlueSphere to check for instructions
            var instructions = await _gatewayService.GetDriverInstructions(_startupService.CurrentVehicle.Registration, _startupService.LoggedInDriver.ID, DateTime.Now, DateTime.Now);
            
            // Check if we have anything in the response
            if (instructions.Any())
            {
                // We have a response so check what we need to do (Save/Update/Delete)
                foreach (var instruction in instructions)
                {
                    switch (instruction.SyncState)
                    {
                        case SyncState.Add:
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

        #region To Delete


        //private async Task SubmitQueueAsync()
        //{
        //    Mvx.Trace("Submit Queue");

        //    if (_isSubmitting)
        //    {
        //        _submitAgainOnCompletion = true;
        //        return;
        //    }

        //    _isSubmitting = true;

        //    try
        //    {
        //        if (!_reachability.IsConnected())
        //            return;

        //        var queuedItems = _queueItemRepository.GetAllInQueueOrder().ToList();

        //        foreach (var queuedItem in queuedItems)
        //        {
        //            var submitted = true;

        //            try
        //            {
        //                if (await this.ServiceCallAsync(queuedItem.JsonSerializedRequestContent))
        //                    _queueItemRepository.Delete(queuedItem);
        //                else
        //                    //TODO: write failure to error log or report in some other way?
        //                    submitted = false;
        //            }
        //            catch
        //            {
        //                //TODO: write failure to error log or report in some other way?
        //                submitted = false;
        //            }

        //            //TODO: should we attempt remaining items if one fails or bail out at this point?
        //            if (!submitted)
        //                break;
        //        }
        //    }
        //    finally
        //    {
        //        _isSubmitting = false;
        //    }

        //    if (_submitAgainOnCompletion)
        //    {
        //        _submitAgainOnCompletion = false;
        //        await SubmitQueueAsync();
        //    }
        //}

        //private async Task<bool> ServiceCallAsync(string jsonSerializedRequestContent)
        //{
        //    var response = await this.PostAsync(jsonSerializedRequestContent);
        //    return response.Succeeded && response.Content.Actions.Any() && response.Content.Actions.All(a => a.Ack);
        //}

        //private Task<HttpResult<Models.GatewayServiceResponse.Response>> PostAsync(string jsonSerializedRequestContent)
        //{
        //    return _httpService.PostAsync<Models.GatewayServiceResponse.Response>(jsonSerializedRequestContent, _gatewayDeviceRequestUrl);
        //}

        ///// <summary>
        ///// Create a single-action request's content without data
        ///// </summary>
        //private Content CreateRequestContent(string command, IEnumerable<Parameter> parameters = null)
        //{
        //    return this.CreateRequestContent(new[]
        //    {
        //        new Models.GatewayServiceRequest.Action
        //        {
        //            Command = command,
        //            Parameters = parameters,
        //        }
        //    });
        //}

        ///// <summary>
        ///// Create a single-action request's content with data - the data will be serialized as xml
        ///// </summary>
        //private Content CreateRequestContent<TData>(string command, TData data, IEnumerable<Parameter> parameters = null)
        //    where TData : class
        //{
        //    string xmlSerializedData = XmlSerialize(data);

        //    return this.CreateRequestContent(new[]
        //    {
        //        new Models.GatewayServiceRequest.Action<string>
        //        {
        //            Command = command,
        //            Data = xmlSerializedData,
        //            Parameters = parameters,
        //        }
        //    });
        //}

        /// <summary>
        /// Create the request content, allowing multiple actions per request
        /// </summary>
        //private Content CreateRequestContent(IEnumerable<Models.GatewayServiceRequest.Action> actions)
        //{
        //    Models.Device device = _deviceRepository.GetAll().FirstOrDefault();
        //    var deviceIdentifier = device == null ? _deviceInfo.GetDeviceIdentifier() : device.DeviceIdentifier;

        //    return new Content
        //    {
        //        DeviceIdentifier = deviceIdentifier,
        //        Password = _deviceInfo.GatewayPassword,
        //        MobileApplication = _deviceInfo.MobileApplication,
        //        Actions = actions,
        //    };
        //}

        //private static string XmlSerialize<TData>(TData data)
        //    where TData : class
        //{
        //    var serializer = new XmlSerializer(typeof(TData));
        //    var settings = new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true };
        //    var namespaces = new XmlSerializerNamespaces();
        //    namespaces.Add(string.Empty, string.Empty);

        //    var stringBuilder = new StringBuilder();

        //    using (var xmlWriter = System.Xml.XmlWriter.Create(stringBuilder, settings))
        //    {
        //        serializer.Serialize(xmlWriter, data, namespaces);
        //    }

        //    return stringBuilder.ToString();
        //}

        #endregion

        private void PublishTimerCommand(Messages.GatewayPollTimerCommandMessage.TimerCommand timerCommand)
        {
            _messenger.Publish(new Messages.GatewayPollTimerCommandMessage(this, timerCommand));
        }

    }

}
