using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class StaticTimerControlTest
	{
		[TestMethod]
		public void ConstructorShouldInitializeProperty()
		{
			byte value = 0xFF;
			StaticControlBits timerControl = new StaticControlBits(value);

			// Assert
			Assert.AreEqual<byte>(value, timerControl.ByteData, "Constructor value not set on ByteData property.");
		}

		[TestMethod]
		public void ByteDataShouldSetCorrectProperties()
		{
			byte value = 0xFF;
			StaticControlBits timerControl = new StaticControlBits(value);

			// Assert
			Assert.IsTrue(timerControl.EnableCount, "Count should be enabled.");
			Assert.IsTrue(timerControl.EnableInterrupt, "Interrupt should be enabled.");
			Assert.IsTrue(timerControl.EnableReload, "Reloading should be enabled.");
			Assert.IsTrue(timerControl.MagMode, "Mag mode should be set.");
			Assert.IsTrue(timerControl.ResetTimerDone, "Reset timer done should be set.");
			Assert.AreEqual<ClockSelect>(ClockSelect.Linking, timerControl.SourcePeriod, "Source period should be set to Linking.");
		}

		[TestMethod]
		public void ByteDataShouldSetCorrectProperties2()
		{
			byte value = 0x00;
			StaticControlBits timerControl = new StaticControlBits(value);

			// Assert
			Assert.IsFalse(timerControl.EnableCount, "Count should be disabled.");
			Assert.IsFalse(timerControl.EnableInterrupt, "Interrupt should be disabled.");
			Assert.IsFalse(timerControl.EnableReload, "Reloading should be disabled.");
			Assert.IsFalse(timerControl.MagMode, "Mag mode should be cleared.");
			Assert.IsFalse(timerControl.ResetTimerDone, "Reset timer done should be cleared.");
			Assert.AreEqual<ClockSelect>(ClockSelect.us1, timerControl.SourcePeriod, "Source period should be 1 microsecond.");
		}
	}
}
