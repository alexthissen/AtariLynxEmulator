using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for SpriteCollisionNumberTest
	/// </summary>
	[TestClass]
	public class SpriteCollisionNumberTest
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

		byte byteData = 0x2f;

		[TestMethod]
		public void ConstructorShouldSetByteData()
		{
			SpriteCollisionNumber sprcoll = new SpriteCollisionNumber(byteData);

			Assert.AreEqual<byte>(byteData, sprcoll.ByteData, "Constructor should set ByteData property.");
		}

		[TestMethod]
		public void PropertyGettersShouldReturnCorrectValues()
		{
			SpriteCollisionNumber sprcoll = new SpriteCollisionNumber(byteData);

			Assert.AreEqual<int>(15, sprcoll.Number, "Collision number should be set to 15.");
			Assert.IsTrue(sprcoll.DontCollide, "DontCollide bit should have been set.");
		}

		[TestMethod]
		public void PropertySettersShouldSetCorrectByteData()
		{
			SpriteCollisionNumber sprcoll = new SpriteCollisionNumber(0);
			sprcoll.DontCollide = true;
			sprcoll.Number = 15;

			Assert.AreEqual<byte>(byteData, sprcoll.ByteData, "Properties should have set correct ByteData.");
		}

		[TestMethod]
		public void TooLargeCollisionNumberShouldBeTruncated()
		{
			SpriteCollisionNumber sprcoll = new SpriteCollisionNumber(0);
			
			// Act
			sprcoll.Number = 0x1f;

			// Assert 
			Assert.AreEqual<int>(0x0f, sprcoll.Number, "Collision number should be truncated to value below 16.");
		}
	}
}
