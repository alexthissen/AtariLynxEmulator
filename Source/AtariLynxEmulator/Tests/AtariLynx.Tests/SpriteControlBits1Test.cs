using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KillerApps.Emulation.Atari.Lynx;

namespace AtariLynx.Tests
{
	/// <summary>
	/// Summary description for SpriteControlBits1Test
	/// </summary>
	[TestClass]
	public class SpriteControlBits1Test
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
    public void ProperyGettersShouldReturnCorrectValues()
    {
    	SpriteControlBits1 sprctl1 = new SpriteControlBits1(0x00);

			Assert.IsFalse(sprctl1.ReusePalette, "Palette should be marked for reuse.");
			Assert.IsFalse(sprctl1.SkipSprite, "Sprite should be marked as Skip.");
			Assert.AreEqual<ReloadableDepth>(ReloadableDepth.None, sprctl1.ReloadableDepth, "Reloadable depth should be None.");
			Assert.AreEqual<SizingAlgorithm>(SizingAlgorithm.Adder, sprctl1.SizingAlgorithm, "Sizing algorithm should be Adder.");
			Assert.IsFalse(sprctl1.StartDrawingLeft, "Drawing should start right.");
			Assert.IsFalse(sprctl1.StartDrawingUp, "Drawing should start down.");
		}

		[TestMethod]
		public void ProperyGettersShouldReturnCorrectValues2()
		{
			SpriteControlBits1 sprctl1 = new SpriteControlBits1(0xFF);

			Assert.IsTrue(sprctl1.ReusePalette, "Palette should be marked for reload.");
			Assert.IsTrue(sprctl1.SkipSprite, "Sprite should be marked as Skip.");
			Assert.AreEqual<ReloadableDepth>(ReloadableDepth.HVST, sprctl1.ReloadableDepth, "Reloadable depth should be HSizeVSizeStretchTilt.");
			Assert.AreEqual<SizingAlgorithm>(SizingAlgorithm.Shifter, sprctl1.SizingAlgorithm, "Sizing algorithm should be Shifter.");
			Assert.IsTrue(sprctl1.StartDrawingLeft, "Drawing should start right.");
			Assert.IsTrue(sprctl1.StartDrawingUp, "Drawing should start down.");
		}
	}
}
