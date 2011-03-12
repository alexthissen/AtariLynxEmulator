using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for TimerTest
	/// </summary>
	[TestClass]
	public class TimerTest
	{
		private Timer reloadingTimer;
		private Timer nonReloadingTimer;
		private Timer clockedTimer;
		private Timer linkingTimer;
		private Timer expiringTimer;

		private const byte interruptMask = 0x01;
		private const byte currentValue = 0xFF;
		private const byte backupValue = 0x42;

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize() 
		{
			expiringTimer = new Timer(interruptMask) { StaticControlBits = new StaticTimerControl(0x08), CurrentValue = 0 };
			reloadingTimer = new Timer(interruptMask) { StaticControlBits = new StaticTimerControl(0x18), CurrentValue = 0, BackupValue = backupValue };
			nonReloadingTimer = new Timer(interruptMask) { StaticControlBits = new StaticTimerControl(0x08), CurrentValue = 0 };
			clockedTimer = new Timer(interruptMask) { StaticControlBits = new StaticTimerControl(0x08), CurrentValue = currentValue };
			linkingTimer = new Timer(interruptMask) { StaticControlBits = new StaticTimerControl(0x0F), CurrentValue = currentValue };
		}

		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion
		
		[TestMethod]
		public void ConstructorShouldCreateSubObjects()
		{
			Timer timer = new Timer(interruptMask);

			Assert.IsNotNull(timer.DynamicControlBits, "Dynamic control object not created.");
			Assert.IsNotNull(timer.StaticControlBits, "Static control object not created.");
			Assert.AreEqual<byte>(interruptMask, timer.InterruptMask);
		}

		[TestMethod]
		public void ResetShouldSetPropertiesToZero()
		{
			Timer timer = new Timer(interruptMask);
			timer.CurrentValue = currentValue;
			timer.BackupValue = backupValue;
			timer.StaticControlBits = new StaticTimerControl(0xFF);
			timer.DynamicControlBits.TimerDone = true;
			
			// Act 
			timer.Reset();

			// Assert
			Assert.AreEqual<byte>(0, timer.StaticControlBits.ByteData, "Static control did not reset to 0.");
			Assert.AreEqual<byte>(0, timer.DynamicControlBits.ByteData, "Dynamic control did not reset to 0.");
			Assert.AreEqual<byte>(0, timer.CurrentValue, "Current value of timer did not reset to 0.");
			Assert.AreEqual<byte>(0, timer.BackupValue, "Backup value of timer did not reset to 0.");
			Assert.AreEqual<byte>(interruptMask, timer.InterruptMask, "Reset should not change interrupt mask.");
		}

		[TestMethod]
		public void SettingLinkingSourcePeriodShouldSetLinkedTimerLogic()
		{
			Timer timer = new Timer(interruptMask);
			timer.StaticControlBits = new StaticTimerControl(0x07);

			Assert.IsInstanceOfType(timer.TimerLogic, typeof(LinkingTimerLogic), "Linked timer logic not set for Linking source period.");
		}

		[TestMethod]
		public void SettingClockSourcePeriodShouldSetClockedTimerLogic()
		{
			Timer timer = new Timer(interruptMask);
			timer.StaticControlBits = new StaticTimerControl(0x01);

			Assert.IsInstanceOfType(timer.TimerLogic, typeof(ClockedTimerLogic), "Clocked timer logic not set for non-Linking source period.");
		}

		[TestMethod]
		public void ExpiringTimerShouldSignalExpirationWithCorrectMask()
		{
			// Arrange 
			Timer timer = new Timer(interruptMask);
			timer.CurrentValue = 0;
			timer.StaticControlBits = new StaticTimerControl(0x88); // Enable interrupt and count
			Assert.IsTrue(timer.StaticControlBits.EnableInterrupt, "Interrupt not enabled before Act.");
			
			byte receivedInterruptMask = 0;
			bool interruptReceived = false;
			timer.Expired += delegate(object sender, TimerExpirationEventArgs e) { receivedInterruptMask = e.InterruptMask; interruptReceived = true; };
			timer.Start(0);
			
			// Act
			timer.Update(16); // 16 clock cycles is 1 microsecond ^= 1 period of clock select

			// Assert
			Assert.IsTrue(interruptReceived, "No interrupt received.");
			Assert.AreEqual<byte>(interruptMask, receivedInterruptMask, "Wrong interrupt mask received from interrupt arguments.");
		}

		[TestMethod]
		public void DisabledTimerShouldNotUpdateCurrentValue()
		{
			// Arrange
			clockedTimer.StaticControlBits = new StaticTimerControl(0); // Disabled timer
			clockedTimer.Start(0);

			// Act
			clockedTimer.Update(ulong.MaxValue);

			// Assert
			Assert.AreEqual<byte>(currentValue, clockedTimer.CurrentValue, "Current value should not change for disabled timer.");
		}

		[TestMethod]
		public void WhenTimerDoneIsSetTimerShouldNotPredictNewTimerEvent()
		{
			clockedTimer.DynamicControlBits.TimerDone = true;
			clockedTimer.Start(0);
			
			clockedTimer.Update(16);

			Assert.AreEqual<ulong>(ulong.MaxValue, clockedTimer.ExpirationTime, "Timer should not predict new timer event when TimerDone flag is set.");
		}

		[TestMethod]
		public void ReloadingTimerShouldReloadBackupValueAndPredictNewTimerEvent()
		{
			// Arrange
			Timer timer = reloadingTimer;
			timer.Start(0);

			// Act
			timer.Update(16);

			Assert.AreEqual<byte>(backupValue, timer.CurrentValue, "Backup value not reloaded in current value.");
			
			// TODO: Check if this condition is true
			Assert.IsTrue(timer.DynamicControlBits.TimerDone, "Reloading timer should set TimerDone flag.");
			// TODO: Presumably backup value should be loaded 1 period after reaching zero (+1 on backup value)
			Assert.AreEqual<ulong>(16 + (backupValue + 1) * 16, timer.ExpirationTime, "Predicted timer event not correct.");
			//Assert.AreEqual<ulong>(16 + backupValue * 16, timer.ExpirationTime, "Predicted timer event not correct.");
		}

		[TestMethod]
		public void UpdateShouldCalculateCorrectCurrentValue()
		{
			Timer timer = clockedTimer;
			timer.Start(0);
			byte decrease = 0x04;

			timer.Update((ulong)(decrease * 16));

			Assert.AreEqual<byte>((byte)(currentValue - decrease), timer.CurrentValue, "Current value not correctly set on update.");
		}

		[TestMethod]
		public void UpdateShouldSetBorrowInFlag()
		{
			Timer timer = clockedTimer;
			timer.Start(0);
			byte decrease = 0x01;

			// Act
			timer.Update((ulong)(decrease * 16));

			// Assert
			Assert.IsTrue(timer.DynamicControlBits.BorrowIn, "Borrow-in value not correctly set on update.");
		}

		[TestMethod]
		public void UpdateWithoutExpirationShouldNotPredictNewExpirationTime()
		{
			Timer timer = clockedTimer;
			timer.Start(0);
			timer.Update(0);
			
			// Get current prediction of expiration
			ulong timerEvent = timer.ExpirationTime;

			timer.Update(16);

			Assert.AreEqual<ulong>(timerEvent, timer.ExpirationTime, "Timer event should not change for same static control values.");
		}

		[TestMethod]
		public void NonReloadingTimerShouldExpireWithZeroCurrentValue()
		{
			// Arrange 
			Timer timer = nonReloadingTimer;
			timer.Start(0);

			// Act
			timer.Update(16); // 16 clock cycles is 1 microsecond ^= 1 period of clock select

			Assert.IsTrue(timer.DynamicControlBits.BorrowOut, "Timer did not trigger.");
			Assert.AreEqual<byte>(0x00, timer.CurrentValue, "Non-reloading timer should have zero current value after trigger.");
		}

		[TestMethod]
		public void ExpiringTimerShouldSetBorrowOutAndTimerDoneFlag()
		{
			// Arrange 
			Timer timer = expiringTimer;
			timer.Start(0);

			// Act
			timer.Update(16); // 16 clock cycles is 1 microsecond ^= 1 period of clock select

			// Assert
			Assert.IsTrue(timer.DynamicControlBits.BorrowOut, "Borrow-out flag not set by triggering timer.");
			Assert.IsTrue(timer.DynamicControlBits.TimerDone, "TimerDone flag not set by triggering timer.");
		}

		[TestMethod]
		public void LinkingTimerShouldDecreaseByOneWhenLinkedTimerExpires()
		{
			Timer timer = linkingTimer;
			timer.PreviousTimer = expiringTimer;
			ulong currentCycleCount = 16;
			expiringTimer.Update(currentCycleCount);
			
			// Act
			linkingTimer.Update(currentCycleCount);

			Assert.AreEqual<byte>(currentValue - 1, linkingTimer.CurrentValue, "Current value should decrease by one when linked timer expires.");
		}

		[TestMethod]
		public void LinkingTimerShouldNotDecreaseWhenLinkedTimerDoesNotExpire()
		{
			Timer timer = linkingTimer;
			timer.PreviousTimer = clockedTimer;
			ulong currentCycleCount = 16;
			clockedTimer.Update(currentCycleCount);

			// Act
			linkingTimer.Update(currentCycleCount);

			Assert.AreEqual<byte>(currentValue, linkingTimer.CurrentValue, "Current value should not decrease when linked timer does not expire.");
		}

		[TestMethod]
		public void ClockedTimerShouldDecreaseCurrentValue()
		{
			Timer timer = clockedTimer;
			timer.Start(0);

			timer.Update(16);

			Assert.IsTrue(timer.CurrentValue < currentValue, "Current value has not been decreased by update.");
		}

		[TestMethod]
		public void UpdateWithoutExpirationShouldClearBorrowOutFlag()
		{
			Timer timer = clockedTimer;
			timer.Start(0);

			timer.Update(16);

			// Assert 
			Assert.IsFalse(timer.DynamicControlBits.BorrowOut, "Borrow-out flag should be cleared if timer did not expire during update.");
		}

		[TestMethod]
		public void UpdateWithoutDecreaseShouldClearBorrowInAndOutFlag()
		{
			Timer timer = clockedTimer;
			timer.Start(0);

			// Act
			timer.Update(1); // Too little timer to expire timer

			// Assert 
			Assert.IsFalse(timer.DynamicControlBits.BorrowIn, "Borrow-in flag should be cleared if timer did not decrease.");
			Assert.IsFalse(timer.DynamicControlBits.BorrowOut, "Borrow-out flag should be cleared if timer did not decrease.");
		}
		
		[TestMethod]
		public void ClockedTimerShouldSetTimerExpiration()
		{
			Timer timer = clockedTimer;
			timer.Start(0);

			// Act
			timer.Update(0);
		}

		[TestMethod]
		public void ReloadingTimerWithMissedExpirationShouldSetExpirationTimeAfterCurrentCycleCount()
		{
			Timer timer = reloadingTimer;
			ulong period = 16;
			ulong interval = (1 + backupValue + 1) * period; // 1 for first expiration, backup value + 1 for second expiration
			timer.Start(0);

			// Act
			timer.Update(interval); 

			// Assert
			Assert.AreEqual<ulong>(period + 1, timer.ExpirationTime, "Missed expiration should mean an immediate expiration again.");
		}
		
		[TestMethod]
		[Ignore] // TODO: Find out what functionality of ResetTimerDone flag is
		public void ReloadingTimerWithResetTimerDoneFlagSetShouldClearTimerDoneFlag()
		{
			
		}
	}
}
