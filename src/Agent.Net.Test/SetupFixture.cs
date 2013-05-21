using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gibraltar.Agent.Net.Test
{
    [TestClass]
    public class SetupFixture
    {
        [AssemblyInitialize]
        public static void Setup(TestContext context)
        {
            Log.Initializing += LogOnInitializing;
            Log.StartSession("Starting Unit Tests");
        }

        [AssemblyCleanup]
        public static void Shutdown()
        {
            Log.EndSession("Closing Unit Tests");
        }

        private static void LogOnInitializing(object sender, LogInitializingEventArgs e)
        {
            e.Configuration.Publisher.ProductName = "Loupe";
            e.Configuration.Publisher.ApplicationName = "Network Agent Tests";

            AssemblyFileVersionAttribute assemblyFileVersion = (AssemblyFileVersionAttribute)Assembly.GetAssembly(typeof(ConnectivityMonitor)).GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault();
            e.Configuration.Publisher.ApplicationVersion = new Version(assemblyFileVersion.Version);
        }
    }
}
