using Gibraltar.Agent.Metrics;

namespace Gibraltar.Agent.Net.Internal
{
    /// <summary>
    /// Creates an event metric for connectivity information
    /// </summary>

    [EventMetric("Loupe", ConnectivityMonitor.LogCategory, "Availability", 
        Description = "Tracks the availability of an endpoint via TCP")]
    internal class ConnectivityEventMetric
    {
        public ConnectivityEventMetric(string ipAddress, bool isAvailable)
        {
            IpAddress = ipAddress;
            IsAvailable = isAvailable;
        }

        [EventMetricInstanceName]
        public string IpAddress { get; private set; }

        [EventMetricValue("isAvailable", SummaryFunction.Average, "%",Caption = "Is Available", 
            Description = "Indicates of the connection is available", IsDefaultValue = true)]
        public bool IsAvailable { get; private set; }
    }
}
