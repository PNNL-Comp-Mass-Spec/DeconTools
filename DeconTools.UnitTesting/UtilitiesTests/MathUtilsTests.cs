using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.UtilitiesTests2
{
    [TestFixture]
    public class MathUtilsTests
    {

        public void getInterpolatedValueTest1()
        {
            double interpolatedVal = MathUtils.getInterpolatedValue(500.5, 500.834, 15, 45, 500.6);

            Assert.AreEqual(23.9820359281439, (decimal)interpolatedVal);
        }



    }
}
