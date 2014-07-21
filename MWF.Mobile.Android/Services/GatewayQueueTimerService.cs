using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace MWF.Mobile.Android.Services
{
    
    /// <summary>
    /// An Android service that will run a timer in the background that handles triggering submission of the gateway queue.
    /// </summary>
    /// <remarks>Communication with the rest of the app (receiving commands and triggering the queue submissions) is done via the messenger.</remarks>
    [Service]
    public class GatewayQueueTimerService : Service
    {

        private const double _timerIntervalMilliseconds = 60000;

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private volatile bool _requestStop = false;

        private IMvxMessenger _messenger;
        private MvxSubscriptionToken _commandMessageToken;

        public void Stop()
        {
            _requestStop = true;
            _timer.Stop();
        }

        public void Start()
        {
            _requestStop = false;
            _timer.Start();
        }

        public void Trigger()
        {
            // Run timer elapsed event immediately and restart timer
            this.Stop();
            this.PublishTimerMessage();
            this.Start();
        }

        public override void OnStart(Intent intent, int startId)
        {
            base.OnStart(intent, startId);

            _messenger = Mvx.Resolve<IMvxMessenger>();
            _commandMessageToken = _messenger.Subscribe<Core.Messages.GatewayQueueTimerCommandMessage>(m => HandleCommandMessage(m));

            _timer.Interval = _timerIntervalMilliseconds;
            _timer.AutoReset = false;

            _timer.Elapsed += (s, e) =>
                {
                    try
                    {
                        this.PublishTimerMessage();
                    }
                    finally
                    {
                        // Restart the timer
                        if (!_requestStop)
                        {
                            _timer.Interval = _timerIntervalMilliseconds;
                            _timer.Start();
                        }
                    }
                };
        }

        private void HandleCommandMessage(Core.Messages.GatewayQueueTimerCommandMessage message)
        {
            switch (message.Command)
            {
                case Core.Messages.GatewayQueueTimerCommandMessage.TimerCommand.Stop:
                    this.Stop();
                    break;
                case Core.Messages.GatewayQueueTimerCommandMessage.TimerCommand.Start:
                    this.Start();
                    break;
                case Core.Messages.GatewayQueueTimerCommandMessage.TimerCommand.Trigger:
                    this.Trigger();
                    break;
            }
        }

        private void PublishTimerMessage()
        {
            _messenger.Publish(new Core.Messages.GatewayQueueTimerElapsedMessage(this));
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

    }

}