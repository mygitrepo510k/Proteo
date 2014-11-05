using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Newtonsoft.Json;
using MWF.Mobile.Core.Repositories;
using System.Xml.Serialization;

namespace MWF.Mobile.Core.Services
{
    
    public class GatewayQueuedService : IGatewayQueuedService
    {

        private readonly IDeviceInfo _deviceInfo = null;
        private readonly IHttpService _httpService = null;
        private readonly Repositories.IGatewayQueueItemRepository _queueItemRepository = null;
        private readonly Portable.IReachability _reachability = null;
        private readonly IDeviceRepository _deviceRepository;

        private readonly IMvxMessenger _messenger = null;

        private readonly string _gatewayDeviceRequestUrl = null;

        private MvxSubscriptionToken _queueTimerMessageToken = null;
        private bool _isSubmitting = false;
        private bool _submitAgainOnCompletion = false;
        
        public GatewayQueuedService(IDeviceInfo deviceInfo, IHttpService httpService, Portable.IReachability reachability, IRepositories repositories, IMvxMessenger messenger)
        {
            _deviceInfo= deviceInfo;
            _httpService = httpService;
            _queueItemRepository = repositories.GatewayQueueItemRepository;
            _reachability = reachability;
            _messenger = messenger;

            //TODO: read this from config or somewhere?
            _gatewayDeviceRequestUrl = "http://87.117.243.226:7090/api/gateway/devicerequest";

            _deviceRepository = repositories.DeviceRepository;
        }

        public void StartQueueTimer()
        {
            if (_queueTimerMessageToken == null)
                _queueTimerMessageToken = _messenger.Subscribe<Messages.GatewayQueueTimerElapsedMessage>(async m => await SubmitQueueAsync());

            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Start);
        }

        public void StopQueueTimer()
        {
            if (_queueTimerMessageToken != null)
            {
                _queueTimerMessageToken.Dispose();
                _queueTimerMessageToken = null;
            }

            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Stop);
        }

        /// <summary>
        /// Add a single-action command without data to the queue and trigger submission to the MWF Mobile gateway web service
        /// </summary>
        /// <remarks>
        /// Note that submission will only occur if the GatewayQueueTimerService has been started (i.e. by first calling StartQueueTimer())
        /// </remarks>
        public void AddToQueue(string command, Models.GatewayServiceRequest.Parameter[] parameters = null)
        {
            this.AddToQueue(CreateRequestContent(command, parameters));
        }

        /// <summary>
        /// Add a single-action command with data to the queue and trigger submission to the MWF Mobile gateway web service
        /// </summary>
        /// <remarks>
        /// Note that submission will only occur if the GatewayQueueTimerService has been started (i.e. by first calling StartQueueTimer())
        /// </remarks>
        public void AddToQueue<TData>(string command, TData data, Models.GatewayServiceRequest.Parameter[] parameters = null)
            where TData: class
        {
            this.AddToQueue(CreateRequestContent(command, data, parameters));
        }

        public void AddToQueue<TData>(IEnumerable<Models.GatewayServiceRequest.Action<TData>> actions) where TData : class
        {
            this.AddToQueue(CreateRequestContent(actions));
        }

        private void AddToQueue(Models.GatewayServiceRequest.Content requestContent)
        {
            var serializedContent = JsonConvert.SerializeObject(requestContent);
            var queueItem = new Models.GatewayQueueItem { ID = Guid.NewGuid(), JsonSerializedRequestContent = serializedContent, QueuedDateTime = DateTime.Now };
            _queueItemRepository.Insert(queueItem);

            // Always attempt to sync with the MWF Mobile Gateway service whenever items are added to the queue (providing the GatewayQueueTimerService has been started)
            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Trigger);
        }

        public async Task UploadQueue()
        {
            PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand.Reset);
            await SubmitQueueAsync();
        }

        private async Task SubmitQueueAsync()
        {
            Mvx.Trace("Submit Queue");

            if (_isSubmitting)
            {
                _submitAgainOnCompletion = true;
                return;
            }

            _isSubmitting = true;

            try
            {
                if (!_reachability.IsConnected())
                    return;

                var queuedItems = _queueItemRepository.GetAllInQueueOrder().ToList();

                foreach (var queuedItem in queuedItems)
                {
                    var submitted = true;

                    try
                    {
                        if (await this.ServiceCallAsync(queuedItem.JsonSerializedRequestContent))
                            _queueItemRepository.Delete(queuedItem);
                        else
                            //TODO: write failure to error log or report in some other way?
                            submitted = false;
                    }
                    catch
                    {
                        //TODO: write failure to error log or report in some other way?
                        submitted = false;
                    }

                    //TODO: should we attempt remaining items if one fails or bail out at this point?
                    if (!submitted)
                        break;
                }
            }
            finally
            {
                _isSubmitting = false;
            }

            if (_submitAgainOnCompletion)
            {
                _submitAgainOnCompletion = false;
                await SubmitQueueAsync();
            }
        }

        private async Task<bool> ServiceCallAsync(string jsonSerializedRequestContent)
        {
            var response = await this.PostAsync(jsonSerializedRequestContent);
            return response.Succeeded && response.Content.Actions.Any() && response.Content.Actions.All(a => a.Ack);
        }

        private Task<HttpResult<Models.GatewayServiceResponse.Response>> PostAsync(string jsonSerializedRequestContent)
        {
            return _httpService.PostJsonAsync<Models.GatewayServiceResponse.Response>(jsonSerializedRequestContent, _gatewayDeviceRequestUrl);
        }

        /// <summary>
        /// Create a single-action request's content with data - the data will be serialized as xml
        /// </summary>
        private Models.GatewayServiceRequest.Content CreateRequestContent<TData>(string command, TData data, IEnumerable<Models.GatewayServiceRequest.Parameter> parameters = null)
            where TData: class
        {
            return this.CreateRequestContent(new[]
            {
                new Core.Models.GatewayServiceRequest.Action<TData>
                {
                    Command = command,
                    Data = data,
                    Parameters = parameters,
                }
            });
        }

        /// <summary>
        /// Create the request content, allowing multiple actions per request and serializing the data
        /// </summary>
        private Models.GatewayServiceRequest.Content CreateRequestContent<TData>(IEnumerable<Models.GatewayServiceRequest.Action<TData>> actions)
            where TData : class
        {
            var xmlSerializedActions = actions.Select(a => new Models.GatewayServiceRequest.Action
            {
                Command = a.Command,
                ContentXml = XmlSerialize(a.Data),
                Parameters = a.Parameters,
            });

            return CreateRequestContent(xmlSerializedActions);
        }

        /// <summary>
        /// Create the request content, allowing multiple actions per request
        /// </summary>
        private Models.GatewayServiceRequest.Content CreateRequestContent(IEnumerable<Models.GatewayServiceRequest.Action> actions)
        {
            Models.Device device = _deviceRepository.GetAll().FirstOrDefault();
            var deviceIdentifier = device == null ? _deviceInfo.GetDeviceIdentifier() : device.DeviceIdentifier;

            return new Core.Models.GatewayServiceRequest.Content
            {
                DeviceIdentifier = deviceIdentifier,
                Password = _deviceInfo.GatewayPassword,
                MobileApplication = _deviceInfo.MobileApplication,
                Actions = actions,
            };
        }

        private static string XmlSerialize<TData>(TData data)
            where TData : class
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TData));
            var settings = new System.Xml.XmlWriterSettings { OmitXmlDeclaration = true };
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var stringBuilder = new StringBuilder();

            using (var xmlWriter = System.Xml.XmlWriter.Create(stringBuilder, settings))
            {
                serializer.Serialize(xmlWriter, data, namespaces);
            }

            return stringBuilder.ToString();
        }

        private void PublishTimerCommand(Messages.GatewayQueueTimerCommandMessage.TimerCommand timerCommand)
        {
            _messenger.Publish(new Messages.GatewayQueueTimerCommandMessage(this, timerCommand));
        }

    }

}
