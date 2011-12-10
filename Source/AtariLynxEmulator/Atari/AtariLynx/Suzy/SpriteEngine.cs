using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Core;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteEngine
	{
		internal Word VIDADR, COLLADR;
		internal Word STRETCH;
		internal Word TILTACUM, TILT;
		internal Word SPRDOFF;
		internal Word SPRVPOS;
		internal Word HSIZACUM, VSIZACUM;
		internal Word TMPADR, SCBADR;
		// "Each SCB also points to the sprite data block containing the image of interest. Many SCBs may point to one sprite data block."
		internal Word PROCADR;

		public bool StretchingEnabled { get; set; }
		public bool SizingEnabled { get; set; }
		public bool TiltingEnabled { get; set; }

		// "The sprite engine consists of several 8 bit control registers, a 16 bit wide sprite 
		// control block register set and ALU, an address manipulator, an 8 byte deep source data FIFO, 
		// a 12 bit shift register for unpacking the data, a 16 nybble pen index palette, 
		// a pixel byte builder, an 8 word deep pixel data FIFO, a data merger, 
		// and assorted control logic."

		internal byte[] ramMemory = null;
		internal byte[] videoMemory = null; // For safe drawing

		// "16 nybble (8 byte) pen index palette specific to each sprite"
		private byte[] PenIndexPalette = new byte[16];
		// "... several 8 bit control registers, a 16 bit wide sprite control block register set"
		private SpriteControlBlock scb;

		private SpriteDataUnpacker unpacker;
		private ShiftRegister shifter; // "a 12 bit shift register for unpacking the data"
		internal SpriteContext context;
		private byte highestCollision;

		public SpriteControlBlock SpriteControlBlock
		{
			get { return scb; }
		}

		public SpriteEngine(SpriteContext context, byte[] ramMemory, SpriteControlBlock scb) :
			this(context, ramMemory, scb, ramMemory)
		{ }

		public SpriteEngine(SpriteContext context, byte[] ramMemory, SpriteControlBlock scb, byte[] videoMemory)
		{
			this.context = context;
			this.videoMemory = videoMemory;
			this.ramMemory = ramMemory;
			this.scb = scb;

			TiltingEnabled = false;
			StretchingEnabled = false;
			SizingEnabled = false;

			Initialize();
		}

		public void OverrideVideo(byte[] videoMemory, ushort videoBaseAddress)
		{
			this.videoMemory = videoMemory;
			this.context.VIDBAS.Value = videoBaseAddress;
		}

		internal void Initialize()
		{
			shifter = new ShiftRegister(12); // "a 12 bit shift register for unpacking the data"
			unpacker = new SpriteDataUnpacker(shifter, ramMemory);
		}

		public int RenderSprites()
		{
			int cyclesUsed = 0;

			// "(0 last SCB)"
			// "The circuit that detects a '0' in the SCB NEXT field of an SCB only looks at the upper byte."
			// "Only the upper byte of the 'NEXT' word in the SCB needs to be set to zero in order to indicate 
			// that this is the last SCB in the list. The lower byte of that word can then be used for any 
			// other function since the hardware will ignore it if the upper byte is zero."

			while ((scb.SCBNEXT.Value & 0xFF00) != 0)
			{
				// Copy current SCB address before reading in SCB
				SCBADR.Value = scb.SCBNEXT.Value;
				TMPADR.Value = SCBADR.Value;

				// "Since all of the SCBs and sprite data blocks are accessed by pointers, they may be located anywhere in RAM space. 
				// Neither SCBs nor sprite data may be located in Mikey ROM."
				if (SCBADR.Value >= 0xFD00 && SCBADR.Value <= 0xFDFF) throw new LynxException("Sprite data is located in Mikey address space.");

				// Parse data to load sprite control block
				// "Each occurrence of a sprite on the screen requires 1 SCB."
				cyclesUsed += InitializeFromSpriteDataStructure(SCBADR.Value);

				// "At the start of painting a particular sprite, a hardware register called fred is cleared to 0."
				highestCollision = 0;

				RenderSingleSprite();

				// "At the end of the processing of this particular sprite, the number in fred will be written out to the 
				// collision depository. If more than one collideable object was hit, the number in fred will be the 
				// HIGHEST of all of the collision numbers detected."
				if (!this.scb.SPRCOLL.DontCollide && !this.context.DontCollide)
				{
					switch (scb.SPRCTL0.SpriteType)
					{
						case SpriteTypes.ExclusiveOrShadow:
						case SpriteTypes.Boundary:
						case SpriteTypes.Normal:
						case SpriteTypes.BoundaryShadow:
						case SpriteTypes.Shadow:
							{
								ushort collisionDepository = (ushort)(SCBADR.Value + context.COLLOFF.Value);
								ramMemory[collisionDepository] = highestCollision;
							}
							break;
						default:
							break;
					}
				}
			}

			//return cyclesUsed;
			return 0;
		}

		public void RenderSingleSprite()
		{
			// "The processing of an actual sprite can be 'skipped' on a sprite by sprite basis."
			if (scb.SPRCTL1.SkipSprite) return;

			bool isEverOnScreen = false;

			PROCADR.Value = scb.SPRDLINE.Value; // Set current PROC address

			// Prepare for unpacking data
			Quadrant quadrant = scb.StartQuadrant;
			unpacker.Initialize(PROCADR.Value, scb.SPRCTL0.BitsPerPixel, scb.SPRCTL1.TotallyLiteral);

			// Loop through all quadrants
			do
			{
				// Quadrant initialization
				// Determine direction for vertical and horizontal drawing
				int horizontalIncrease = quadrant.HorizontalIncrease;
				int verticalIncrease = quadrant.VerticalIncrease;

				if (scb.SPRCTL0.VFlip) verticalIncrease = -verticalIncrease;
				if (scb.SPRCTL0.HFlip) horizontalIncrease = -horizontalIncrease;

				TILTACUM.Value = 0;
				VSIZACUM.Value = (ushort)((verticalIncrease == 1) ? context.VSIZOFF.Value : 0);

				// Calculate current vertical position
				//SPRVPOS.Value = (ushort)((short)scb.VPOSSTRT.Value - context.VOFF.Value);
				int sprvpos = (short)scb.VPOSSTRT.Value - (short)context.VOFF.Value;

				// Initialize current line data value
				scb.SPRDLINE.Value = PROCADR.Value;

				// Fix squashed look by offsetting vertical offset by one
				if (quadrant.VerticalIncrease != scb.StartQuadrant.VerticalIncrease)
					sprvpos += verticalIncrease;

				// Loop through all lines
				byte pixelHeight = 0;
				while ((SPRDOFF.Value = unpacker.ReadOffsetToNextLine()) >= 2)
				{
					// Vertical scaling
					VSIZACUM.Value += scb.SPRVSIZ.Value;
					pixelHeight = VSIZACUM.HighByte;
					VSIZACUM.HighByte = 0;

					VIDADR.Value = (ushort)(context.VIDBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));
					COLLADR.Value = (ushort)(context.COLLBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));

					// TODO: Check visibility

					for (int v = 0; v < pixelHeight; v++)
					{
						// Stop vertical loop if outside of screen bounds
						if (verticalIncrease == 1 && sprvpos >= Suzy.SCREEN_HEIGHT) break;
						if (verticalIncrease == -1 && sprvpos < 0) break;

						if (sprvpos >= 0 && sprvpos < Suzy.SCREEN_HEIGHT)
						{
							scb.HPOSSTRT.Value += (ushort)((short)TILTACUM.Value >> 8);
							TILTACUM.HighByte = 0;
							HSIZACUM.Value = (ushort)((horizontalIncrease == 1) ? context.HSIZOFF.Value : 0);
							int sprhpos = (short)scb.HPOSSTRT.Value - (short)context.HOFF.Value;

							// Fix squashed look by offsetting 1 pixel on other directions
							if (quadrant.HorizontalIncrease != scb.StartQuadrant.HorizontalIncrease)
								sprhpos += horizontalIncrease;

							// Draw row of pixels
							foreach (byte pixelIndex in unpacker.PixelsInLine((byte)(SPRDOFF.Value - 1)))
							{
								HSIZACUM.Value += scb.SPRHSIZ.Value;
								byte pixelWidth = HSIZACUM.HighByte;
								HSIZACUM.HighByte = 0;

								// Draw pixel
								byte pixelValue = PenIndexPalette[pixelIndex];

								for (int h = 0; h < pixelWidth; h++)
								{
									// Stop horizontal loop if outside of screen bounds
									if (sprhpos >= 0 && sprhpos < Suzy.SCREEN_WIDTH)
									{
										// Process pixel based on sprite type
										ProcessPixel((ushort)(VIDADR.Value + (sprhpos + sprvpos * Suzy.SCREEN_WIDTH) / 2), pixelValue, sprhpos % 2 == 0);
										ProcessCollision((ushort)(COLLADR.Value + (sprhpos + sprvpos * Suzy.SCREEN_WIDTH) / 2), pixelValue, sprhpos % 2 == 0);
										
										isEverOnScreen = true;
									}
									sprhpos += horizontalIncrease;
								}
							}
						}
						sprvpos += verticalIncrease;

						// "The horizontal size of a sprite can be modified every time a scan line is processed. 
						// This allows for 'stretching' a sprite and in conjunction with 'tilt' can be useful in creating 
						// arbitrary polygons."
						if (StretchingEnabled) scb.SPRHSIZ.Value += STRETCH.Value;

						// "The horizontal position of a sprite can be modified every time a scan line is processed. 
						// This allows for 'tilting' a sprite and in conjunction with 'stretch' can be useful in 
						// creating arbitrary polygons."
						if (TiltingEnabled) TILTACUM.Value += TILT.Value;
					}

					unpacker.MoveToNextLine((byte)(SPRDOFF.Value - 1));
					scb.SPRDLINE.Value += SPRDOFF.Value;

					// "The vertical size of a sprite can be modified every time a scan line is processed. 
					// This allows for 'stretching' a sprite vertically. The vertical stretch factor is the same 
					// as the horizontal stretch factor.										
					// "Vertical stretching can be enabled on a sprite by sprite basis."
					if (context.VStretch) scb.SPRVSIZ.Value += (ushort)(STRETCH.Value * pixelHeight);
				}

				// Check if all quadrant rendering is done
				if (SPRDOFF.Value == 0) break;

				// "An offset of +1 instructs the hardware to change to the next drawing direction."
				// Switch to next quadrant
				quadrant = GetNextQuadrant(quadrant);
			}
			while (quadrant != scb.StartQuadrant); // Never more than 4 quadrants

			if (context.EveronEnabled)
			{
				scb.SPRCOLL.Everon = isEverOnScreen;
			}
		}

		// "Each SCB contains certain elements in a certain order as required by the hardware"
		public int InitializeFromSpriteDataStructure(ushort address)
		{
			ushort startAddress = address;

			scb.SPRCTL0.ByteData = ramMemory[address++]; // "(1 byte)  8 bits of control (SPRCTLO)"
			scb.SPRCTL1.ByteData = ramMemory[address++]; // "(1 byte)  8 bits of control (SPRCTL1)"
			scb.SPRCOLL.ByteData = ramMemory[address++]; // "(1 byte)  4 bits of control (SPRCOLL)"
			// "(2 bytes) 16 bits of pointer to next sprite SCB (0 last SCB)"
			if (BitConverter.IsLittleEndian)
			{
				scb.SCBNEXT.Value = BitConverter.ToUInt16(ramMemory, address);
				address += 2;
			}
			else
			{
				scb.SCBNEXT.Value = (ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
			}

			// "The processing of an actual sprite can be 'skipped' on a sprite by sprite basis."
			if (!scb.SPRCTL1.SkipSprite)
			{
				if (BitConverter.IsLittleEndian)
				{
					// "(2 bytes) 16 bits of address of start of Sprite Data"
					scb.SPRDLINE.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
					// "(2) 16 bits of starting H Pos"
					scb.HPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
					// "(2) 16 bits of starting V Pos"
					scb.VPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				}
				else
				{
					scb.SPRDLINE.Value = (ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
					scb.HPOSSTRT.Value = (ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
					scb.VPOSSTRT.Value = (ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
				}
				address += ParseReloadableDepth(ramMemory, address);

				// Read pen palette if necessary
				// "The 8 bytes of pen palette are treated by the hardware as a separate block of data from 
				// the previous group of bytes in the SCB. This means that the reloadability of some of the 
				// previous bytes does not affect the reusability of the pen palette. 
				// In addition, this means that when some of the bytes are not reloaded, the length of the 
				// SCB will be smaller by the number of bytes not used."
				if (!scb.SPRCTL1.ReusePalette)
				{
					ReloadPalette(ramMemory, address);
					address += 8;
				}
			}

			return (address - startAddress) * Suzy.SPRITE_READWRITE_CYCLE;
		}

		public void ProcessCollision(ushort address, byte pixel, bool left)
		{
			if (address < context.COLLBAS.Value || address >= (context.COLLBAS.Value + 160 * 102 / 2))
				throw new LynxException("Writing outside of video memory area.");

			if (scb.SPRCOLL.DontCollide || context.DontCollide) return;

			switch (scb.SPRCTL0.SpriteType)
			{
				case SpriteTypes.BackgroundShadow:
					if (!scb.SPRCOLL.DontCollide && !context.DontCollide && pixel != 0x0E)
						WriteCollision(address, scb.SPRCOLL.Number, left);
					break;

				case SpriteTypes.Boundary:
				case SpriteTypes.Normal:
					if (pixel != 0x00)
					{
						byte collision = ReadCollision(address, left);

						// "In the course of painting this particular sprite, as each pixel is painted, the corresponding 
						// 'cell' in the collision buffer is read (actually done in bursts of 8 pixels). 
						// If the number read from the collision buffer cell is larger than the number currently in fred, ..."
						if (collision > highestCollision)
						{
							// "... then this larger number will be stored in fred."
							highestCollision = collision;
						}
						WriteCollision(address, scb.SPRCOLL.Number, left);
					}
					break;

				case SpriteTypes.ExclusiveOrShadow:
					if (pixel != 0x00 && pixel != 0x0E)
					{
						if (!scb.SPRCOLL.DontCollide && !context.DontCollide && pixel != 0x0E)
						{
							// Read collision and set collision number if necessary
							byte collision = ReadCollision(address, left);
							if (collision > highestCollision)
							{
								highestCollision = collision;
							}
							WriteCollision(address, scb.SPRCOLL.Number, left);
						}
					}
					break;

				case SpriteTypes.Shadow:
				case SpriteTypes.BoundaryShadow:
					if (pixel != 0x00 && pixel != 0x0E)
					{
						// Read collision and set collision number if necessary
						byte collision = ReadCollision(address, left);
						if (collision > highestCollision)
						{
							highestCollision = collision;
						}
						WriteCollision(address, scb.SPRCOLL.Number, left);
					}
					break;

				case SpriteTypes.NonCollidable:
				case SpriteTypes.BackgroundNoCollision:
				default:
					break;
			}
		}

		public void ProcessPixel(ushort address, byte pixel, bool left)
		{
			switch (scb.SPRCTL0.SpriteType)
			{
				case SpriteTypes.BackgroundShadow:
				case SpriteTypes.BackgroundNoCollision:
					WritePixel(address, pixel, left);
					break;

				case SpriteTypes.BoundaryShadow:
					if (pixel != 0x00 && pixel != 0x0E && pixel != 0x0F)
					{
						WritePixel(address, pixel, left);
					}
					break;

				case SpriteTypes.Boundary:
					if (pixel != 0x00 && pixel != 0x0F)
					{
						WritePixel(address, pixel, left);
					}
					break;

				case SpriteTypes.Normal:
				case SpriteTypes.NonCollidable:
				case SpriteTypes.Shadow:
					if (pixel != 0x00) WritePixel(address, pixel, left);
					break;

				case SpriteTypes.ExclusiveOrShadow:
					if (pixel != 0x00)
					{
						byte value = ReadPixel(address, left);
						value ^= pixel;
						WritePixel(address, value, left);
					}
					break;

				default:
					break;
			}
		}

		private byte ReadPixel(ushort address, bool left)
		{
			if (address < context.VIDBAS.Value || address >= (context.VIDBAS.Value + 160 * 102 / 2))
				throw new LynxException("Writing outside of video memory area.");

			byte value = videoMemory[address];
			if (left)
			{
				// Upper nibble screen read
				value >>= 4;
			}
			else
			{
				// Lower nibble screen read
				value &= 0x0f;
			}

			// TODO: Increase cycle count
			//cycles_used += SPR_RDWR_CYC;
			return value;
		}

		private byte ReadCollision(ushort address, bool left)
		{
			if (address < context.COLLBAS.Value || address >= (context.COLLBAS.Value + 160 * 102 / 2))
				throw new LynxException("Writing outside of video memory area.");

			byte value = videoMemory[address];
			if (left)
			{
				// Upper nibble screen read
				value >>= 4;
			}
			else
			{
				// Lower nibble screen read
				value &= 0x0f;
			}

			// TODO: Increase cycle count
			//cycles_used += SPR_RDWR_CYC;
			return value;
		}

		private void WriteCollision(ushort address, byte pixel, bool left)
		{
			if (address < context.COLLBAS.Value || address >= (context.COLLBAS.Value + 160 * 102 / 2))
				throw new LynxException("Writing outside of video memory area.");

			byte value = videoMemory[address];
			if (left)
			{
				// Upper nibble screen write
				value &= 0x0f;
				value |= (byte)(pixel << 4);
			}
			else
			{
				// Lower nibble screen write
				value &= 0xf0;
				value |= pixel;
			}
			videoMemory[address] = value;

			// TODO: Increase cycle count
			//cycles_used += 2 * SPR_RDWR_CYC;
		}

		public void WritePixel(ushort address, byte pixel, bool left)
		{
			if (address < context.VIDBAS.Value || address >= (context.VIDBAS.Value + 160 * 102 / 2))
				throw new LynxException("Writing outside of video memory area.");

			byte value = videoMemory[address];
			if (left)
			{
				// Upper nibble screen write
				value &= 0x0f;
				value |= (byte)(pixel << 4);
			}
			else
			{
				// Lower nibble screen write
				value &= 0xf0;
				value |= pixel;
			}
			videoMemory[address] = value;

			// TODO: Increase cycle count
			//cycles_used += 2 * SPR_RDWR_CYC;
		}

		private Quadrant GetNextQuadrant(Quadrant current)
		{
			QuadrantOrder order = (current.Order == QuadrantOrder.DownLeft) ? QuadrantOrder.DownRight : current.Order + 1;
			return SpriteControlBlock.Quadrants[(int)order];
		}

		private ushort ParseReloadableDepth(byte[] memory, ushort address)
		{
			ushort bytesRead = 0;
			TiltingEnabled = StretchingEnabled = SizingEnabled = false;
			TILT.Value = 0;
			STRETCH.Value = 0;
			//scb.SPRHSIZ.Value = scb.SPRVSIZ.Value = 0;

			switch (scb.SPRCTL1.ReloadableDepth)
			{
				case ReloadableDepth.None:
					break;

				case ReloadableDepth.HVST:
					TiltingEnabled = true;
					// "(2 bytes) 16 bits of tilt value"
					
					TILT.Value = BitConverter.IsLittleEndian ? 
						BitConverter.ToUInt16(memory, address + 6) :
						(ushort)((ramMemory[address + 7] << 8) + ramMemory[address + 6]);
					bytesRead += 2;
					goto case ReloadableDepth.HVS;

				case ReloadableDepth.HVS:
					StretchingEnabled = true;
					// "(2 bytes) 16 bits of stretch value"
					STRETCH.Value = BitConverter.IsLittleEndian ?
						BitConverter.ToUInt16(memory, address + 4) :
						(ushort)((ramMemory[address + 5] << 8) + ramMemory[address + 4]);
					bytesRead += 2;
					goto case ReloadableDepth.HV;

				case ReloadableDepth.HV:
					SizingEnabled = true;
					// "(2 bytes) 16 bits of H size bits"
					if (BitConverter.IsLittleEndian)
					{
						scb.SPRHSIZ.Value = BitConverter.ToUInt16(memory, address);
						address += 2;
					}
					else
					{
						scb.SPRHSIZ.Value = (ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
					}
					// "(2 bytes) 16 bits of V size bits"
					scb.SPRVSIZ.Value = BitConverter.IsLittleEndian ?
						BitConverter.ToUInt16(memory, address) : 
						(ushort)(ramMemory[address++] + (ramMemory[address++] << 8));
					bytesRead += 4;
					break;

				default:
					break;
			}

			return bytesRead;
		}

		private void ReloadPalette(byte[] memory, ushort address)
		{
			// "(8 bytes) 64 bits of pen palette"
			PenIndexPalette = new byte[16];
			for (int index = 0; index < 8; index++)
			{
				byte value = memory[address + index];
				PenIndexPalette[2 * index] = (byte)((value >> 4) & 0x0F);
				PenIndexPalette[2 * index + 1] = (byte)(value & 0x0F);
			}
		}
	}
}
