using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Core;
using KillerApps.Emulation.Atari.Lynx.Tooling;

namespace KillerApps.Emulation.Atari.Lynx
{
	[DebuggerVisualizer(typeof(SuzyVisualizer))]
	[Serializable]
	public class SpriteEngine
	{
		public Word VIDADR, COLLADR;
		public Word STRETCH;
		public Word TILTACUM, TILT;
		public Word SPRDOFF;
		public Word SPRVPOS;
		public Word HSIZACUM, VSIZACUM;
		public Word TMPADR, SCBADR;
		// "Each SCB also points to the sprite data block containing the image of interest. Many SCBs may point to one sprite data block."		
		public Word PROCADR;

		public bool StretchingEnabled { get; set; }
		public bool SizingEnabled { get; set; }
		public bool TiltingEnabled { get; set; }

		// "The sprite engine consists of several 8 bit control registers, a 16 bit wide sprite 
		// control block register set and ALU, an address manipulator, an 8 byte deep source data FIFO, 
		// a 12 bit shift register for unpacking the data, a 16 nybble pen index palette, 
		// a pixel byte builder, an 8 word deep pixel data FIFO, a data merger, 
		// and assorted control logic."

		byte[] ramMemory = null;
		byte[] videoMemory = null; // For safe drawing
		ShiftRegister shifter; // "a 12 bit shift register for unpacking the data"
		// "16 nybble (8 byte) pen index palette specific to each sprite"
		byte[] PenIndexPalette = new byte[16];
		// "... several 8 bit control registers, a 16 bit wide sprite control block register set"
		SpriteControlBlock scb;
		SpriteDataUnpacker unpacker;
		private SpriteContext context;

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
			shifter = new ShiftRegister(12); // "a 12 bit shift register for unpacking the data"
			unpacker = new SpriteDataUnpacker(shifter, ramMemory);
			this.scb = scb;

			TiltingEnabled = false;
			StretchingEnabled = false;
			SizingEnabled = false;
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
				cyclesUsed += InitializeFromSpriteDataStructure(ramMemory, SCBADR.Value);

				RenderSingleSprite();
			}

			return cyclesUsed; 
		}

		// "Each SCB contains certain elements in a certain order as required by the hardware"
		public int InitializeFromSpriteDataStructure(byte[] memory, ushort address)
		{
			ushort startAddress = address;

			scb.SPRCTL0.ByteData = memory[address++]; // "(1 byte)  8 bits of control (SPRCTLO)"
			scb.SPRCTL1.ByteData = memory[address++]; // "(1 byte)  8 bits of control (SPRCTL1)"
			scb.SPRCOLL.ByteData = memory[address++]; // "(1 byte)  4 bits of control (SPRCOLL)"
			// "(2 bytes) 16 bits of pointer to next sprite SCB (0 last SCB)"
			scb.SCBNEXT.Value = BitConverter.ToUInt16(memory, address); address += 2;

			// "The processing of an actual sprite can be 'skipped' on a sprite by sprite basis."
			if (!scb.SPRCTL1.SkipSprite)
			{
				// "(2 bytes) 16 bits of address of start of Sprite Data"
				scb.SPRDLINE.Value = BitConverter.ToUInt16(memory, address); address += 2;
				// "(2) 16 bits of starting H Pos"
				scb.HPOSSTRT.Value = BitConverter.ToUInt16(memory, address); address += 2;
				// "(2) 16 bits of starting V Pos"
				scb.VPOSSTRT.Value = BitConverter.ToUInt16(memory, address); address += 2;
				address += ParseReloadableDepth(memory, address);
			}

			// Read pen palette if necessary
			// "The 8 bytes of pen palette are treated by the hardware as a separate block of data from 
			// the previous group of bytes in the SCB. This means that the reloadability of some of the 
			// previous bytes does not affect the reusability of the pen palette. 
			// In addition, this means that when some of the bytes are not reloaded, the length of the 
			// SCB will be smaller by the number of bytes not used."
			if (scb.SPRCTL1.ReloadPalette)
			{
				ReloadPalette(memory, address);
				address += 8;
			}

			return (address - startAddress) * Suzy.SPRITE_READWRITE_CYCLE;
		}

		public void RenderSingleSprite()
		{
			// "The processing of an actual sprite can be 'skipped' on a sprite by sprite basis."
			if (scb.SPRCTL1.SkipSprite) return;

			PROCADR.Value = scb.SPRDLINE.Value; // Set current PROC address

			// Prepare for unpacking data
			QuadrantOrder quadrant = scb.StartQuadrant;
			unpacker.Initialize(PROCADR.Value, scb.SPRCTL0.BitsPerPixel, scb.SPRCTL1.TotallyLiteral);

			// Loop through all quadrants
			do
			{
				// Quadrant initialization
				// Determine direction for vertical and horizontal drawing
				int horizontalIncrease = ((quadrant == QuadrantOrder.DownRight || quadrant == QuadrantOrder.UpRight) ? 1 : -1);
				int verticalIncrease = (quadrant == QuadrantOrder.UpRight || quadrant == QuadrantOrder.UpLeft) ? -1 : 1;

				TILTACUM.Value = 0;
				VSIZACUM.Value = (ushort)((verticalIncrease == 1) ? context.VSIZOFF.Value : 0);

				// Calculate current vertical position
				//SPRVPOS.Value = (ushort)((short)scb.VPOSSTRT.Value - context.VOFF.Value);
				int sprvpos = (short)scb.VPOSSTRT.Value - (short)context.VOFF.Value;

				// Initialize current line data value
				scb.SPRDLINE.Value = PROCADR.Value;

				// TODO: Fix squashed look by offsetting vertical offset by one

				// Loop through all lines
				while ((SPRDOFF.Value = unpacker.ReadOffsetToNextLine()) >= 2)
				{
					// Vertical scaling
					VSIZACUM.Value += scb.SPRVSIZ.Value;
					byte pixelHeight = VSIZACUM.HighByte;
					VSIZACUM.HighByte = 0;

					VIDADR.Value = (ushort)(context.VIDBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));
					COLLADR.Value = (ushort)(context.COLLBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));

					// TODO: Check visibility

					scb.HPOSSTRT.Value += (ushort)((short)TILTACUM.Value >> 8);
					TILTACUM.HighByte = 0;
					HSIZACUM.Value = (ushort)((horizontalIncrease == 1) ? context.HSIZOFF.Value : 0);
					ushort sprhpos = (ushort)((short)scb.HPOSSTRT.Value - context.HOFF.Value);

					// TODO: Fix squashed look by offsetting 1 pixel on other directions

					// Draw row of pixels
					foreach (byte pixelIndex in unpacker.PixelsInLine((byte)(SPRDOFF.Value - 1)))
					{
						HSIZACUM.Value += scb.SPRHSIZ.Value;
						byte pixelWidth = HSIZACUM.HighByte;
						HSIZACUM.HighByte = 0;

						// Draw pixel
						byte pixelValue = PenIndexPalette[pixelIndex];
						int vpos = sprvpos;

						for (int v = 0; v < pixelHeight; v++)
						{
							// Stop vertical loop if outside of screen bounds
							if (vpos < 0 || vpos >= Suzy.SCREEN_HEIGHT) break;

							int hpos = sprhpos;
							for (int h = 0; h < pixelWidth; h++)
							{
								// Stop horizontal loop if outside of screen bounds
								if (hpos < 0 || hpos >= Suzy.SCREEN_WIDTH) break;

								// TODO: Process pixel based on sprite type
								ProcessPixel((ushort)(VIDADR.Value + (hpos + vpos * Suzy.SCREEN_WIDTH) / 2), pixelValue, hpos % 2 == 0);

								hpos = (ushort)(hpos + horizontalIncrease);
							}
							vpos += verticalIncrease;
						}
						sprhpos += (ushort)(horizontalIncrease * pixelWidth);
					}
					scb.SPRDLINE.Value += SPRDOFF.Value;
					//SPRVPOS.Value = (ushort)((short)SPRVPOS.Value + verticalIncrease * pixelHeight);
					sprvpos += verticalIncrease * pixelHeight;

					if (StretchingEnabled) scb.SPRHSIZ.Value += STRETCH.Value;
					if (context.VStretch) scb.SPRVSIZ.Value += (ushort)(STRETCH.Value * pixelHeight);
					if (TiltingEnabled) TILTACUM.Value += TILT.Value;
				}

				// Check if all quadrant rendering is done
				if (SPRDOFF.Value == 0) break;

				// "An offset of +1 instructs the hardware to change to the next drawing direction."
				// Switch to next quadrant
				quadrant = GetNextQuadrant(quadrant);
			}
			while (quadrant != scb.StartQuadrant); // Never more than 4 quadrants
		}

		public void ProcessPixel(ushort address, byte pixel, bool left)
		{
			switch (scb.SPRCTL0.SpriteType)
			{
				case SpriteTypes.BackgroundShadow:
					WritePixel(address, pixel, left);
					if (!scb.SPRCOLL.DontCollide && !context.DontCollide && pixel != 0x0E)
					{
						WriteCollision(address, scb.SPRCOLL.Number, left);
					}
					break;

				case SpriteTypes.BackgroundNoCollision:
					WritePixel(address, pixel, left);
					break;

				case SpriteTypes.BoundaryShadow:
					if (pixel != 0x00 && pixel != 0x0e && pixel != 0x0f)
					{
						WritePixel(address, pixel, left);
					}
					if (pixel != 0x00 && pixel != 0x0e)
					{
						if (!scb.SPRCOLL.DontCollide && !context.DontCollide)
						{
							// TODO: Read collision and set collision number if necessary
						}
					}
					break;

				case SpriteTypes.Boundary:
					if (pixel != 0x00 && pixel != 0x0f)
					{
						WritePixel(address, pixel, left);
					}
					if (pixel != 0x00)
					{
						if (!scb.SPRCOLL.DontCollide && !context.DontCollide)
						{
							// TODO: Read collision and set collision number if necessary
						}
					}
					break;

				case SpriteTypes.Normal:
					if (pixel != 0x00)
					{
						WritePixel(address, pixel, left);
						if (pixel != 0x00)
						{
							if (!scb.SPRCOLL.DontCollide && !context.DontCollide)
							{
								// TODO: Read collision and set collision number if necessary
							}
						}
					}
					break;

				case SpriteTypes.NonCollidable:
					if (pixel != 0x00) WritePixel(address, pixel, left);
					break;

				case SpriteTypes.ExclusiveOrShadow:
					if (pixel != 0x00)
					{
						byte value = ReadPixel(address, left);
						value ^= pixel;
						WritePixel(address, value, left);
					}
					if (pixel != 0x00 && pixel != 0x0E)
					{
						if (!scb.SPRCOLL.DontCollide && !context.DontCollide && pixel != 0x0E)
						{
							// TODO: Read collision and set collision number if necessary
						}
					}
					break;

				case SpriteTypes.Shadow:
					if (pixel != 0x00) WritePixel(address, pixel, left);
					if (pixel != 0x00 && pixel != 0x0e)
					{
						if (!scb.SPRCOLL.DontCollide && !context.DontCollide)
						{
							// TODO: Read collision and set collision number if necessary
						}
					}
					break;

				default:
					break;
			}
		}

		private byte ReadPixel(ushort address, bool left)
		{
			byte value = ramMemory[address];
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

		private void WriteCollision(ushort address, int pixel, bool left)
		{

		}

		public void WritePixel(ushort address, byte pixel, bool left)
		{
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

		private QuadrantOrder GetNextQuadrant(QuadrantOrder current)
		{
			return (current == QuadrantOrder.DownLeft) ? QuadrantOrder.DownRight : ++current;
		}

		private ushort ParseReloadableDepth(byte[] memory, ushort address)
		{
			ushort bytesRead = 0;
			switch (scb.SPRCTL1.ReloadableDepth)
			{
				case ReloadableDepth.None:
					break;

				case ReloadableDepth.HVST:
					TiltingEnabled = true;
          // "(2 bytes) 16 bits of tilt value"
					TILT.Value = BitConverter.ToUInt16(memory, address + 6);
					bytesRead += 2;
					goto case ReloadableDepth.HVS;

				case ReloadableDepth.HVS:
					StretchingEnabled = true;
					// "(2 bytes) 16 bits of stretch value"
					STRETCH.Value = BitConverter.ToUInt16(memory, address + 4);
					bytesRead += 2;
					goto case ReloadableDepth.HV;

				case ReloadableDepth.HV:
					SizingEnabled = true;
					// "(2 bytes) 16 bits of H size bits"
					scb.SPRHSIZ.Value = BitConverter.ToUInt16(memory, address); address += 2;
					// "(2 bytes) 16 bits of V size bits"
					scb.SPRVSIZ.Value = BitConverter.ToUInt16(memory, address);
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
