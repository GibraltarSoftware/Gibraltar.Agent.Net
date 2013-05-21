﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using Gibraltar.Agent.Metrics;
using Gibraltar.Agent.Net.Internal;

namespace Gibraltar.Agent.Net
{
    /// <summary>
    /// Monitors an IP address for availability
    /// </summary>
    public class ConnectivityMonitor: IDisposable
    {
        /// <summary>
        /// The default number of seconds to wait for a respone to our ping before deciding connectivity has been lost.
        /// </summary>
        public const int DefaultTimeout = 10;

        /// <summary>
        /// The default number of consecutive failures before the connection will be determined down.
        /// </summary>
        public const int DefaultRetries = 3;

        /// <summary>
        /// The default number of seconds between connectivity checks
        /// </summary>
        public const int DefaultInterval = 30;

        /// <summary>
        /// The string we use to create a payload for our ping, in case someone is looking
        /// </summary>
        private const string PingContent = "Gibraltar Loupe Connection Tests"; //exactly 32 chars

        internal const string LogCategory = "System.Net.Connectivity";

        private readonly string _ipAddress;
        private readonly object _lock = new object();
        private readonly Ping _pingSender;
        private readonly byte[] _pingPayload;
        private readonly Timer _timer;

        private volatile bool _isAccessible; //volatile to avoid locks on frequently queried values
        private int _failureCount;
        private bool _isMonitoring;
        private bool _inRequest;
        private int _timeout;
        private int _retries;
        private int _checkInterval;
        private PingOptions _pingOptions;

        /// <summary>
        /// Raised whenever the status of the monitored endpoint changes
        /// </summary>
        public event EventHandler IsAccessibleChanged;

        /// <summary>
        /// Create a new connectivity monitor for the specified IP address or hostname
        /// </summary>
        /// <param name="ipAddress"></param>
        public ConnectivityMonitor(string ipAddress)
        {
            _ipAddress = ipAddress;
            _isAccessible = false;
            _retries = DefaultRetries;
            _timeout = DefaultTimeout;
            _pingSender = new Ping();
            _pingOptions = new PingOptions(_timeout * 1000, true);
            _pingSender.PingCompleted += OnPingCompleted;
            _pingPayload = Encoding.ASCII.GetBytes(PingContent);
            _timer = new Timer(AsyncPollConnection);
            _checkInterval = DefaultInterval;

            //we want to be extra responsive if we are told we lost network..
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChange;
        }

        /// <summary>
        /// The Ip Address or host name being monitored
        /// </summary>
        public string IpAddress { get { return _ipAddress; } }

        /// <summary>
        /// True when the address was accessible when last checked.
        /// </summary>
        public bool IsAccessible { get { return _isAccessible; } }

        /// <summary>
        /// The number of seconds between connectivity checks.
        /// </summary>
        public int Interval
        {
            get
            {
                lock(_lock)
                {
                    return _checkInterval;
                }
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("The interval must be a positive integer greater than zero");

                lock(_lock)
                {
                    _checkInterval = value;
                }
            }
        }

        /// <summary>
        /// The number of consecutive failures before the connection will be determined down.
        /// </summary>
        public int Retries
        {
            get
            {
                lock(_lock)
                {
                    return _retries;
                }
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("The number of retries must be a positive integer greater than zero");

                lock (_lock)
                {
                    _retries = value;
                }
            }
        }

        /// <summary>
        /// The number of seconds to wait for a response
        /// </summary>
        public int Timeout
        {
            get
            {
                lock(_lock)
                {
                    return _timeout;
                }
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("The timeout of retries must be a positive integer greater than zero");

                lock (_lock)
                {
                    _timeout = value;
                    _pingOptions = new PingOptions(_timeout * 1000, false);
                }
            }
        }

        public int FailureCount { get { return _failureCount; } }

        /// <summary>
        /// True when the address is being monitored.
        /// </summary>
        /// <remarks>Set True to start monitoring the address, false to stop</remarks>
        public bool IsMonitored
        {
            get
            {
                lock(_lock)
                {
                    return _isMonitoring;
                }
            }
            set { SetMonitor(value); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool releaseManagedResources)
        {
            if (releaseManagedResources)
            {
                NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChange;
                lock(_lock)
                {
                    _isMonitoring = false;
                    _timer.Dispose();
                }
            }
        }

        protected virtual void OnIsAccessibleChanged()
        {
            EventHandler handler = IsAccessibleChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void SetMonitor(bool value)
        {
            lock(_lock)
            {
                if (_isMonitoring == value)
                    return;

                _isMonitoring = value;

                if (_isMonitoring)
                {
                    //we need to *start* the monitor
                    _timer.Change(0, System.Threading.Timeout.Infinite);
                }
                else
                {
                    _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); //disable the timer
                }
            }
        }

        private void AsyncPollConnection(object state)
        {
            try //we are called strait from the threadpool so if we let failures fly it'll fail the process.
            {
                lock(_lock)
                {
                    _inRequest = true;
                    _pingSender.SendAsync(_ipAddress, _timeout * 1000, _pingPayload, _pingOptions, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Connectivity Monitor: Unable to poll connection due to {0}.\r\nIP Address being tested: {1}", ex.GetType(), IpAddress);

                lock(_lock)
                {
                    _inRequest = false;
                }

                //we have to re-queue the timer
                QueueNextCheck();

                GC.KeepAlive(ex); // we don't want to pollute people's stream with the exception details but we want the object around if we're debugging
            }
        }

        private void OnNetworkAvailabilityChange(object sender, NetworkAvailabilityEventArgs e)
        {
            //if we *are* monitoring and aren't in the middle of a check we want to check right now..
            lock(_lock)
            {
                if ((_inRequest == false) && (_isMonitoring))
                {
                    _timer.Change(0, System.Threading.Timeout.Infinite);
                }
            }
        }

        private void OnPingCompleted(object sender, PingCompletedEventArgs e)
        {
            long latency = 0;
            bool raiseEvent = false;
            if ((e.Error != null) || (e.Reply.Status != IPStatus.Success))
            {
                //we failed - so is this enough to trip our failure or not?
                lock(_lock)
                {
#if DEBUG
                    if (e.Error != null)    
                        Debug.Print("Connectivity Monitor: Failed in poll due to {0}.\r\nIP Address being tested: {1}\r\nPrevious failures: {2:N0}", e.Error.GetType(), IpAddress, _failureCount);
                    else
                        Debug.Print("Connectivity Monitor: Failed in poll due to {0}.\r\nIP Address being tested: {1}\r\nPrevious failures: {2:N0}", e.Reply.Status, IpAddress, _failureCount);
#endif
                    _failureCount++;
                    if ((_failureCount > _retries) && (_isAccessible))
                    {
                        _isAccessible = false;
                        raiseEvent = true;
                    }
                }
            }
            else
            {
                latency = e.Reply.RoundtripTime;

                lock(_lock)
                {
                    _failureCount = 0;

                    if (_isAccessible == false)
                    {
                        _isAccessible = true;
                        raiseEvent = true;
                    }
                }
            }

            if (raiseEvent)
            {
                //we don't want exceptions thrown by our subscribers to break us.
                try
                {
                    OnIsAccessibleChanged();
                }
                catch (Exception ex)
                {
                    Log.RecordException(ex, LogCategory, true);
                }
            }

            //now decide when we should poll again.
            QueueNextCheck();

            lock(_lock)
            {
                _inRequest = false;                
            }

            RecordStatusMetric(_ipAddress, _isAccessible, raiseEvent, latency);
        }

        /// <summary>
        /// If we are monitoring queue the next timed check
        /// </summary>
        private void QueueNextCheck()
        {
            lock (_lock)
            {
                if (_isMonitoring)
                {
                    int checkInterval;
                    if ((_isAccessible) && (_failureCount < _retries))
                    {
                        //we want to do a more rapid check because if it failed once, it'll probably fail again.
                        checkInterval = 0;
                    }
                    else
                    {
                        checkInterval = _checkInterval * 1000; //convert from seconds to milliseconds
                    }

                    _timer.Change(checkInterval, System.Threading.Timeout.Infinite); //we have to convert seconds to milliseconds
                }
            }
        }

        private void RecordStatusMetric(string ipAddress, bool isAccessible, bool accessibleChanged, long latency)
        {
            //first our sampled metric for latency...
            var metric = SampledMetric.Register("Loupe", LogCategory, "latency", SamplingType.RawCount, "ms", 
                "Latency", "The latency of the connection to this endpoint (if available)", ipAddress);

            if (isAccessible)
            {
                metric.WriteSample(latency);
            }
            else
            {
                //write a zero latency sample, but credit it to the timestamp where we initiated not now.
                metric.WriteSample(0, DateTimeOffset.Now.AddMilliseconds(-1 * latency));
            }

            if (accessibleChanged)
            {
                EventMetric.Write(new ConnectivityEventMetric(ipAddress, isAccessible));
            }
        }
    }
}
