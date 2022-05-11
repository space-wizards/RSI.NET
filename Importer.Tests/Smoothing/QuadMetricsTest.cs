using System;
using SixLabors.ImageSharp;
﻿using RSI.Smoothing;
using NUnit.Framework;

namespace Importer.Tests.Smoothing
{
    [TestFixture]
    [TestOf(typeof(QuadMetrics))]
    public class QuadMetricsTest
    {
        [Test]
        [TestCase(QuadSubtileIndex.SouthEast, 16, 16)]
        [TestCase(QuadSubtileIndex.NorthWest, 0, 0)]
        [TestCase(QuadSubtileIndex.NorthEast, 16, 0)]
        [TestCase(QuadSubtileIndex.SouthWest, 0, 16)]
        public void GetRowsAndColumnsTest(QuadSubtileIndex qsi, int x, int y)
        {
            var res = new QuadMetrics(new Size(32, 32))[qsi];

            Assert.That(res.X, Is.EqualTo(x));
            Assert.That(res.Y, Is.EqualTo(y));
            Assert.That(res.Width, Is.EqualTo(16));
            Assert.That(res.Height, Is.EqualTo(16));
        }
    }
}
