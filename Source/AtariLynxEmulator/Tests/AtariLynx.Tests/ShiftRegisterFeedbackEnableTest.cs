using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for ShiftRegisterFeedbackEnableTest
	/// </summary>
	[TestClass]
	public class ShiftRegisterFeedbackEnableTest
	{
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
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void ConstructorShouldSetByteData()
		{
			byte data = 0xFF;

			// Act
			ShiftRegisterFeedbackEnable srfe = new ShiftRegisterFeedbackEnable(data);
			
			// Assert
			Assert.AreEqual<byte>(data, srfe.ByteData, "Constructor should set ByteData property to argument value");
		}

		[TestMethod]
		public void SettingByteDataShouldSetProperties()
		{
			// Arrange
			ShiftRegisterFeedbackEnable srfe = new ShiftRegisterFeedbackEnable(0x00);

			// Act
			srfe.ByteData = 0xFF;

			// Assert
			Assert.IsTrue(srfe.FeedbackBit11, "Feedback bit 11 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit10, "Feedback bit 10 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit5, "Feedback bit 5 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit4, "Feedback bit 4 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit3, "Feedback bit 3 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit2, "Feedback bit 2 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit1, "Feedback bit 1 should be enabled.");
			Assert.IsTrue(srfe.FeedbackBit0, "Feedback bit 0 should be enabled.");
		}
	}
}
