#region File Header and License
// /*
//    SetupFixture.cs
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
