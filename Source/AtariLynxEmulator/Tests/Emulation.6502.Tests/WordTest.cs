using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Processors.Tests
{
	/// <summary>
	/// Summary description for WordTest
	/// </summary>
	[TestClass]
	public class WordTest
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
		public void WordSetShouldSetHighAndLowByteCorrectly()
		{
			ushort value = 0x1234;
			Word word = new Word();
			word.Value = value;

			Assert.AreEqual<byte>(0x12, word.HighByte, "High byte should be 0x12");
			Assert.AreEqual<byte>(0x34, word.LowByte, "Low byte should be 0x34");
		}

		[TestMethod]
		public void WordGetShouldRetrieveCorrectValue()
		{
			Word word = new Word();
			word.HighByte = 0x12;
			word.LowByte = 0x34;
			
			Assert.AreEqual<ushort>(0x1234, word.Value, "Word value should be 0x1234");
		}
	}
}
