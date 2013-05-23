#region File Header and License
// /*
//    ConnectivityEventMetric.cs
//    Copyright 2013 Gibraltar Software, Inc.
//    
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// */
#endregion

using Gibraltar.Agent.Metrics;

namespace Gibraltar.Agent.Net.Internal
{
    /// <summary>
    /// Creates an event metric for connectivity information
    /// </summary>

    [EventMetric("Loupe", ConnectivityMonitor.MetricCategory, "Availability", 
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
