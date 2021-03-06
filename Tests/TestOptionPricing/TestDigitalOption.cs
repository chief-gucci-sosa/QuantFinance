﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptionPricing;
using Utilities.MarketData;
using Utilities.ExtenstionMethods;

namespace Tests.TestOptionPricing
{
    [TestClass]
    public class TestDigitalOption
    {
        #region fixtures

        private static DigitalOption GetOption(bool isCall, 
                                               bool isAssetSettled,
                                               double strike = 105)
        {
            var expiryDate = new DateTime(2018, 5, 31);
            var option = new DigitalOption("testAsset", expiryDate, strike, isCall, "testCurve", isAssetSettled);
            return option;
        }

        private static SortedList<DateTime, OptionPricingData> GetPricingData()
        {
            var pricingData = new SortedList<DateTime, OptionPricingData>
            {
                { new DateTime(2018, 4, 30), new OptionPricingData(103, 0.15, 0.04, 0.0)},
                { new DateTime(2018, 5, 31), new OptionPricingData(106, 0.15, 0.04, 0.0)},
                { new DateTime(2018, 6, 30), new OptionPricingData(110, 0.15, 0.04, 0.0)}
            };

            return pricingData;
        }

        private static SortedList<DateTime, double> GetPriceDataOnly()
        {
            var allPricingData = GetPricingData();
            return allPricingData.GetPriceSeries();
        }

        #endregion

        #region tests

        [TestMethod]
        public void TestDigitalOptionCallAssetSettled()
        {
            // arrange
            var option = GetOption(true, true);
            var pricingData = new OptionPricingData(100, 0.15, 0.04, 0);
            var currentDate = new DateTime(2016, 3, 30);

            // act
            var result = option.GetPriceBSModel(currentDate, pricingData);

            // assert
            Assert.AreEqual(61.711724, result, 1e-6);
        }

        [TestMethod]
        public void TestDigitalOptionPutAssetSettled()
        {
            // arrange
            var option = GetOption(false, true);
            var pricingData = new OptionPricingData(100, 0.15, 0.04, 0.0);
            var currentDate = new DateTime(2016, 4, 30);

            // act
            var result = option.GetPriceBSModel(currentDate, pricingData);

            // assert
            Assert.AreEqual(38.865210, result, 1e-6);
        }

        [TestMethod]
        public void TestDigitalOptionCallCashSettled()
        {
            // arrange
            var option = GetOption(true, false);
            var pricingData = new OptionPricingData(107, 0.24, 0.03, 0.04);
            var currentDate = new DateTime(2017, 8, 31);

            // act
            var result = option.GetPriceBSModel(currentDate, pricingData);

            // assert
            Assert.AreEqual(0.467494, result, 1e-6);
        }

        [TestMethod]
        public void TestDigitalOptionPutCashSettled()
        {
            // arrange
            var option = GetOption(false, false);
            var pricingData = new OptionPricingData(107, 0.24, 0.03, 0.04);
            var currentDate = new DateTime(2017, 8, 31);

            // act
            var result = option.GetPriceBSModel(currentDate, pricingData);

            // assert
            Assert.AreEqual(0.509378, result, 1e-6);
        }

        [TestMethod]
        public void TestDigitalOptionPutCallParityCashSettled()
        {
            // arrange
            var callOption = GetOption(true, false);
            var putOption = GetOption(false, false);
            var pricingDate = new OptionPricingData(106, 0.12, 0.07, 0.01);
            var currentDate = new DateTime(2017, 5, 31);


            // act
            var callPrice = callOption.GetPriceBSModel(currentDate, pricingDate);
            var putPrice = putOption.GetPriceBSModel(currentDate, pricingDate);
            var sum = callPrice + putPrice;

            // assert
            var paritySum = 0.929526;  // should just be the discount factor
            Assert.AreEqual(paritySum, sum, 1e-6);
        }

        [TestMethod]
        public void TestDigitalOptionPutCallParityAssetSettled()
        {
            // arrange
            var callOption = GetOption(true, true);
            var putOption = GetOption(false, true);
            var pricingData = new OptionPricingData(101, 0.29, 0.04, 0.04);
            var currentDate = new DateTime(2017, 3, 31);

            // act
            var callPrice = callOption.GetPriceBSModel(currentDate, pricingData);
            var putPrice = putOption.GetPriceBSModel(currentDate, pricingData);
            var sum = callPrice + putPrice;

            // assert
            var paritySum = 96.204922;  // should be discounted forward rate
            Assert.AreEqual(paritySum, sum, 1e-6);
        }

        [TestMethod]
        public void TestGetPayoffCallOptionInTheMoneyAssetSettled()
        {
            // arrange
            var option = GetOption(true, true);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(106, result);
        }

        [TestMethod]
        public void TestGetPayoffCallOptionInTheMoneyCashSettled()
        {
            // arrange
            var option = GetOption(true, false);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestGetPayoffCallOptionOutTheMoneyAssetSettled()
        {
            // arrange
            var option = GetOption(true, true, 108);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestGetPayoffCallOptionOutTheMoneyCashSettled()
        {
            // arrange
            var option = GetOption(true, false, 108);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestGetPayoffPutOptionInTheMoneyAssetSettled()
        {
            // arrange
            var option = GetOption(false, true, 108);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(106, result);
        }

        [TestMethod]
        public void TestGetPayoffPutOptionInTheMoneyCashSettled()
        {
            // arrange
            var option = GetOption(false, false, 108);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestGetPayoffPutOptionOutTheMoneyAssetSettled()
        {
            // arrange
            var option = GetOption(false, true, 100);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestGetPayoffPutOptionOutTheMoneyCashSettled()
        {
            // arrange
            var option = GetOption(false, false, 100);
            var prices = GetPriceDataOnly();

            // act
            var result = option.GetPayoff(prices);

            // assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestGetCurrentDeltaCallAssetSettled()
        {
            // arrange
            var option = GetOption(true, true, 108);
            var currentDate = new DateTime(2018, 4, 30);
            var pricingData = GetPricingData();

            // act
            var result = option.GetCurrentDelta(currentDate, pricingData);

            // assert
            Assert.AreEqual(5.820470, result, 1e-6);
        }

        [TestMethod]
        public void TestGetCurrentDeltaPutAssetSettled()
        {
            // arrange
            var option = GetOption(false, true, 108);
            var currentDate = new DateTime(2018, 4, 30);
            var pricingData = GetPricingData();

            // act
            var result = option.GetCurrentDelta(currentDate, pricingData);

            // assert
            Assert.AreEqual(-4.820470, result, 1e-6);
        }

        [TestMethod]
        public void TestGetCurrentDeltaCallCashSettled()
        {
            // arrange
            var option = GetOption(true, false, 108);
            var currentDate = new DateTime(2018, 4, 30);
            var pricingData = GetPricingData();

            // act
            var result = option.GetCurrentDelta(currentDate, pricingData);

            // assert
            Assert.AreEqual(0.052282, result, 1e-6);
        }

        [TestMethod]
        public void TestGetCurrentDeltaPutCashSettled()
        {
            // arrange
            var option = GetOption(false, false, 108);
            var currentDate = new DateTime(2018, 4, 30);
            var pricingData = GetPricingData();

            // act
            var result = option.GetCurrentDelta(currentDate, pricingData);

            // assert
            Assert.AreEqual(-0.0522816, result, 1e-6);
        }

        #endregion 
    }
}
