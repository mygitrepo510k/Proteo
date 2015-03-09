using System;
using Android.App;
using Android.Content;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace MWF.Mobile.Android.Services
{

    /// <summary>
    /// An Android service that will run a timer in the background that handles triggering polling of the gateway.
    /// </summary>
    /// <remarks>Communication with the rest of the app (receiving commands and triggering the queue polling) is done via the messenger.</remarks>
    [Service]
    public class GatewayPollTimerService : Service
    {

        private const double _timerIntervalMilliseconds = 30000;

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

        public void Reset()
        {
            if (!_timer.Enabled)
                return;

            this.Stop();
            this.Start();
        }

        public void Trigger()
        {
            if (!_timer.Enabled)
                return;

            // Run timer elapsed event immediately and restart timer
            this.Stop();
            this.PublishTimerMessage();
            this.Start();
        }

        public override void OnStart(Intent intent, int startId)
        {
            base.OnStart(intent, startId);

            _messenger = Mvx.Resolve<IMvxMessenger>();
            _commandMessageToken = _messenger.Subscribe<Core.Messages.GatewayPollTimerCommandMessage>(m => HandleCommandMessage(m));

            _timer.Interval = _timerIntervalMilliseconds;
            _timer.AutoReset = true;

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

        private void HandleCommandMessage(Core.Messages.GatewayPollTimerCommandMessage message)
        {
            switch (message.Command)
            {
                case Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Stop:
                    this.Stop();
                    break;
                case Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Start:
                    this.Start();
                    break;
                case Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Reset:
                    this.Reset();
                    break;
                case Core.Messages.GatewayPollTimerCommandMessage.TimerCommand.Trigger:
                    this.Trigger();
                    break;
            }
        }

        private void PublishTimerMessage()
        {
            _messenger.Publish(new Core.Messages.GatewayPollTimerElapsedMessage(this));
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

    }

}