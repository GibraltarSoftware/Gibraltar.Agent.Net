#region File Header and License
// /*
//    ConnectivityMonitorTests.cs
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

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gibraltar.Agent.Net.Test
{
    [TestClass]
    public class ConnectivityMonitorTests
    {
        [TestMethod]
        public void MonitorAvailableHostName()
        {
            using (var monitor = new ConnectivityMonitor("www.google.com"))
            {
                monitor.IsMonitored = true;
                
                //we only allow a few seconds for it to spin up and say we're valid..
                Thread.Sleep(5000);

                Assert.IsTrue(monitor.IsAccessible, "The monitor is reporting that '{0}' is not accessible after our sample delay", monitor.IpAddress);
            }
        }

        [TestMethod]
        public void MonitorAvailableIpAddress()
        {
            using (var monitor = new ConnectivityMonitor("8.8.8.8"))
            {
                monitor.IsMonitored = true;

                //we only allow a few seconds for it to spin up and say we're valid..
                Thread.Sleep(5000);

                Assert.IsTrue(monitor.IsAccessible, "The monitor is reporting that '{0}' is not accessible after our sample delay", monitor.IpAddress);
            }
        }

        [TestMethod]
        public void RapidConnectionMonitor()
        {
            using (var monitor = new ConnectivityMonitor("www.google.com"))
            {
                monitor.Interval = 2;
                monitor.Timeout = 1;
                monitor.IsMonitored = true;

                Thread.Sleep(15000); //let it do a number of rounds...

                Assert.IsTrue(monitor.IsAccessible, "The monitor is reporting that '{0}' is not accessible after our sample delay", monitor.IpAddress);
            }
        }
    }
}
