using System;
using DbManager.Logic;
using NUnit.Framework;
using DbManager.Logic.Presenters;
using DbManager.Logic.Interfaces;
using DbManager.Infrastructure;
using Moq;

namespace ntcUnitTestProject
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void WhenChecksumHas32Characters_ReturnTrue()
        {
            var checksum = new ChecksumMD5();
            var filePath = @"C:\Users\Dell\Documents\chinook.db";
            var checksumLength = checksum.CalculateChecksum(filePath);
            Assert.AreEqual(checksumLength.Length, 32);
        }
        [Test]
        public void WhenChecksumFilePathIsEmpty_ThrowEx()
        {
            var checksum = new ChecksumMD5();
            var filePath = "";
            Assert.Throws<System.ArgumentException>(() => checksum.CalculateChecksum(filePath));
        }
    }
}
