using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class UtilityTests
    {
        [Test]
        public void getAssemblyVersionInfoTest1()
        {
            string versionString = AssemblyInfoRetriever.GetVersion(typeof(Task));
            Console.WriteLine("Version = " +versionString);
            Assert.Greater(versionString.Length, 10);
        }


        
    }
}
