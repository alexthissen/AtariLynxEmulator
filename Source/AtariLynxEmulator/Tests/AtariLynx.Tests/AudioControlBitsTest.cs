using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class AudioControlBitsTest
	{
		[TestMethod]
		public void ConstructorShouldSetByteData()
		{
			// Arrange
			byte data = 0xFF;

			// Act 
			AudioControlBits control = new AudioControlBits(data);

			// Assert
			Assert.AreEqual<byte>(data, control.ByteData, "Constructor should pass argument to ByteData property.");
		}

		[TestMethod]
		public void SettingByteDataShouldSetProperties()
		{
			// Arrange
			byte data = 0xFF;
			AudioControlBits control = new AudioControlBits(data);

			// Assert
			Assert.AreEqual<ClockSelect>(ClockSelect.Linking, control.SourcePeriod, "Source period should be set to Linking.");
			Assert.IsTrue(control.FeedbackBit7, "Feedback bit 7 should be enabled.");
			Assert.IsTrue(control.EnableCount, "Clock count should be enabled.");
			Assert.IsTrue(control.EnableIntegrateMode, "Integrated mode should be enabled.");
			Assert.IsTrue(control.EnableReload, "Reloading of clock should be enabled.");
			Assert.IsTrue(control.ResetTimerDone, "Resetting clock when timer is done should be enabled.");
		}

		[TestMethod]
		public void ClearingByteDataShouldSetProperties()
		{
			// Arrange
			byte data = 0x00;
			AudioControlBits control = new AudioControlBits(data);

			// Assert
			Assert.AreEqual<ClockSelect>(ClockSelect.us1, control.SourcePeriod, "Source period should be set to 1 microsecond.");
			Assert.IsFalse(control.FeedbackBit7, "Feedback bit 7 should be disabled.");
			Assert.IsFalse(control.EnableCount, "Clock count should be disabled.");
			Assert.IsFalse(control.EnableIntegrateMode, "Integrated mode should be disabled.");
			Assert.IsFalse(control.EnableReload, "Reloading of clock should be disabled.");
			Assert.IsFalse(control.ResetTimerDone, "Resetting clock when timer is done should be disabled.");
		}
	}
}
