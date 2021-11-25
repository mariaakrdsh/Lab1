using Microsoft.VisualStudio.TestTools.UnitTesting;
using LabCalculator;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabCalculator.Tests
{
    [TestClass()]
    public class CalculatorTests
    {
        [TestMethod()]
        public void Power()
        {
            string numberr = "7";
            string power = "0";
            double expected = 1;

            //act

            double actual = Calculator.Evaluate(numberr + "^" + power);

            //assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void IncDec()
        {
            string numberr = "10";
            double expected = 11;

            //act

            double actual = Calculator.Evaluate("inc(" + numberr + ")");

            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ModDiv()
        {
            string numberr = "5";
            string mod = "0";
            double expected = double.PositiveInfinity;

            //act

            double actual = Calculator.Evaluate(numberr + "mod" + mod);

            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}