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
