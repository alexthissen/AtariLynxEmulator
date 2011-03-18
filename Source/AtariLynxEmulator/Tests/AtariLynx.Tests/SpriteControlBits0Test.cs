using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	[TestClass]
	public class SpriteControlBits0Test
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

		private byte byteData = 0xFF;

		[TestMethod]
		public void ConstructorShouldInitializeProperties()
		{
			// Act
			SpriteControlBits0 scb0 = new SpriteControlBits0(byteData);
			
			// Assert
			Assert.AreEqual(scb0.ByteData, byteData, "Byte data should be same as constructor data");
		}

		[TestMethod]
		public void PropertyGettersShouldReturnCorrectValues()
		{
			// Act
			SpriteControlBits0 scb0 = new SpriteControlBits0(0xFF);

			// Assert
			Assert.IsTrue(scb0.HFlip, "Horizontal flip should not be set.");
			Assert.IsTrue(scb0.VFlip, "Horizontal flip should be set.");
			Assert.AreEqual<int>(4, scb0.BitsPerPixel, "Bits per pixel should be 4.");
			Assert.AreEqual<SpriteTypes>(SpriteTypes.Shadow, scb0.SpriteType, "Sprite type should be Shadow.");
		}

		[TestMethod]
		public void PropertyGettersShouldReturnCorrectValues2()
		{
			// Act
			SpriteControlBits0 scb0 = new SpriteControlBits0(0x00);

			// Assert
			Assert.IsFalse(scb0.HFlip, "Horizontal flip should not be set.");
			Assert.IsFalse(scb0.VFlip, "Horizontal flip should be set.");
			Assert.AreEqual<int>(1, scb0.BitsPerPixel, "Bits per pixel should be 1.");
			Assert.AreEqual<SpriteTypes>(SpriteTypes.BackgroundShadow, scb0.SpriteType, "Sprite type should be Background, shadow.");
		}
	}
}
