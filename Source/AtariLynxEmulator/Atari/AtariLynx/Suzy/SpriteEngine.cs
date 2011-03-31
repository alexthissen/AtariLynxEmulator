using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
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
		ShiftRegister shifter; // "a 12 bit shift register for unpacking the data"
		// "16 nybble (8 byte) pen index palette specific to each sprite"
		byte[] PenIndexPalette = new byte[16];
		// "... several 8 bit control registers, a 16 bit wide sprite control block register set"
		SpriteControlBlock scb;
		SpriteDataUnpacker unpacker;
		private Suzy suzy;

		public SpriteEngine(Suzy suzy, byte[] memory, SpriteControlBlock scb)
		{
			this.suzy = suzy;
			ramMemory = memory;
			shifter = new ShiftRegister(12); // "a 12 bit shift register for unpacking the data"
			unpacker = new SpriteDataUnpacker(shifter, ramMemory);
			this.scb = scb;

			TiltingEnabled = false;
			StretchingEnabled = false;
			SizingEnabled = false;
		}

		public ulong RenderSprites() 
		{
			ulong cyclesUsed = 0;

			// "(0 last SCB)"
			// "The circuit that detects a '0' in the SCB NEXT field of an SCB only looks at the upper byte."
			// "Only the upper byte of the 'NEXT' word in the SCB needs to be set to zero in order to indicate 
			// that this is the last SCB in the list. The lower byte of that word can then be used for any 
			// other function since the hardware will ignore it if the upper byte is zero."
			while ((scb.SCBNEXT.Value & 0xFF00) != 0)
			{
				// Set current SCB address before reading in SCB
				SCBADR.Value = scb.SCBNEXT.Value;
				// TMPADR.Value = SCBADR.Value;

				// "Since all of the SCBs and sprite data blocks are accessed by pointers, they may be located anywhere in RAM space. 
				// Neither SCBs nor sprite data may be located in Mikey ROM."
				if (SCBADR.Value >= 0xFD00 && SCBADR.Value <= 0xFDFF) throw new LynxException("Sprite data is located in Mikey address space.");

				// Parse data to load sprite control block
				// "Each occurrence of a sprite on the screen requires 1 SCB."
				cyclesUsed += ParseSpriteControlBlockData(SCBADR.Value);
				PROCADR.Value = scb.SPRDLINE.Value; // Set current PROC address

				// Prepare for unpacking data
				QuadrantOrder quadrant = scb.StartQuadrant;				
				unpacker.Initialize(PROCADR.Value, scb.SPRCTL0.BitsPerPixel, scb.SPRCTL1.TotallyLiteral);

				// Loop through all quadrants
				do
				{
					// Quadrant initialization
					// TODO: Tilt and vertical size initialization
					// TODO: Vertical scaling size offset
					TILTACUM.Value = 0;
					VSIZACUM.Value = suzy.VSIZOFF.Value; // TODO: Make initial value of vertical size accumulator dependent on drawing direction

					// Calculate current vertical position
					SPRVPOS.Value = (ushort)((short)scb.VPOSSTRT.Value - suzy.HOFF.Value);

					// Initialize current line data value
					scb.SPRDLINE.Value = PROCADR.Value;

					// Loop through all lines
					while ((SPRDOFF.Value = unpacker.ReadOffsetToNextLine()) >= 2)
					{
						// Vertical scaling
						VSIZACUM.Value += scb.SPRVSIZ.Value;
						byte pixelHeight = VSIZACUM.HighByte;
						VSIZACUM.HighByte = 0;

						VIDADR.Value = (ushort)(suzy.VIDBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));
						COLLADR.Value = (ushort)(suzy.COLLBAS.Value + (SPRVPOS.Value * (Suzy.SCREEN_WIDTH / 2)));

						// TODO: Check visibility
						// TODO: Set start position
						// TODO: Tilt and scaling

						scb.HPOSSTRT.Value += (ushort)((short)TILTACUM.Value >> 8);
						TILTACUM.HighByte = 0;
						HSIZACUM.Value = suzy.HSIZOFF.Value; // TODO: Make horizontal size accumulator dependent on drawing direction
						ushort sprhpos = (ushort)((short)scb.HPOSSTRT.Value - suzy.HOFF.Value);

						// Draw row of pixels
						foreach (byte pixelIndex in unpacker.PixelsInLine((byte)(SPRDOFF.Value - 1)))
						{
							// TODO: Horizontal scaling
							HSIZACUM.Value += scb.SPRHSIZ.Value;
							byte pixelWidth = HSIZACUM.HighByte;
							HSIZACUM.HighByte = 0;

							// TODO: Draw pixel
							byte pixelValue = PenIndexPalette[pixelIndex];
							ushort sprvpos = 0;
							
							for (int v = 0; v < pixelHeight; v++)
							{
								// Stop vertical loop if outside of screen bounds
								if (sprvpos < 0 || sprvpos >= Suzy.SCREEN_HEIGHT) break;								
								for (int h = 0; h < pixelWidth; h++)
								{
									// Stop horizontal loop if outside of screen bounds
									if (sprhpos < 0 || sprhpos >= Suzy.SCREEN_WIDTH) break;
									
									// TODO: Process pixel based on sprite type
									WritePixel((ushort)(VIDADR.Value + (sprhpos + v * Suzy.SCREEN_WIDTH) / 2), pixelValue, sprhpos % 2 == 0);
									
									sprhpos++; // TODO: Increase or decrease depending on drawing direction
								}

								sprvpos++; // TODO: Increase or decrease depending on drawing direction
							}
						}
						scb.SPRDLINE.Value += SPRDOFF.Value;
						SPRVPOS.Value += pixelHeight;

						//if (StretchingEnabled) scb.SPRHSIZ.Value += STRETCH.Value;
						//if (suzy.SPRSYS.VStretch) scb.SPRVSIZ.Value += (ushort)(STRETCH.Value * pixelHeight);
						//if (TiltingEnabled) TILTACUM.Value += TILT.Value;
					}

					// Check if all quadrant rendering is done
					if (SPRDOFF.Value == 0) break;

					// "An offset of +1 instructs the hardware to change to the next drawing direction."
					// Switch to next quadrant
					quadrant = GetNextQuadrant(quadrant);
				}
				while (quadrant != scb.StartQuadrant); // Never more than 4 quadrants
			}

			return cyclesUsed; 
		}

		public void WritePixel(ushort address, byte pixel, bool left)
		{
			byte value = ramMemory[address];
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
			ramMemory[address] = value;

			// Increment cycle count for the read/modify/write
			//cycles_used += 2 * SPR_RDWR_CYC;
		}

		private QuadrantOrder GetNextQuadrant(QuadrantOrder current)
		{
			return (current == QuadrantOrder.DownLeft) ? QuadrantOrder.DownRight : ++current;
		}

		private ulong ParseSpriteControlBlockData(ushort address)
		{
			ulong cyclesUsed = 0;

			scb.SPRCTL0.ByteData = ramMemory[address++];
			scb.SPRCTL1.ByteData = ramMemory[address++];
			scb.SPRCOLL.ByteData = ramMemory[address++];
			scb.SCBNEXT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
			cyclesUsed += 5 * Suzy.SPRITE_READWRITE_CYCLE;

			if (!scb.SPRCTL1.SkipSprite)
			{
				scb.SPRDLINE.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				scb.HPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				scb.VPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				cyclesUsed = 6 * Suzy.SPRITE_READWRITE_CYCLE;

				address += ParseReloadableDepth(address, ref cyclesUsed);
			}

			// Read pen palette if necessary
			// "The 8 bytes of pen palette are treated by the hardware as a separate block of data from 
			// the previous group of bytes in the SCB. This means that the reloadability of some of the 
			// previous bytes does not affect the reusability of the pen palette. 
			// In addition, this means that when some of the bytes are not reloaded, the length of the 
			// SCB will be smaller by the number of bytes not used."
			if (scb.SPRCTL1.ReloadPalette)
			{
				ReloadPalette(address);
				cyclesUsed = 8 * Suzy.SPRITE_READWRITE_CYCLE;
			}
			return cyclesUsed;
		}

		private ushort ParseReloadableDepth(ushort address, ref ulong cyclesUsed)
		{
			ushort increase = 0;
			switch (scb.SPRCTL1.ReloadableDepth)
			{
				case ReloadableDepth.None:
					break;

				case ReloadableDepth.HVST:
					TiltingEnabled = true;
					TILT.Value = BitConverter.ToUInt16(ramMemory, address + 6);
					increase += 2;
					cyclesUsed += 2 * Suzy.SPRITE_READWRITE_CYCLE;
					goto case ReloadableDepth.HVS;

				case ReloadableDepth.HVS:
					StretchingEnabled = true;
					STRETCH.Value = BitConverter.ToUInt16(ramMemory, address + 4);
					increase += 2;
					cyclesUsed += 2 * Suzy.SPRITE_READWRITE_CYCLE;
					goto case ReloadableDepth.HV;

				case ReloadableDepth.HV:
					SizingEnabled = true;
					scb.SPRHSIZ.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
					scb.SPRVSIZ.Value = BitConverter.ToUInt16(ramMemory, address);
					cyclesUsed += 4 * Suzy.SPRITE_READWRITE_CYCLE;
					increase += 4;
					break;

				default:
					break;
			}

			return increase;
		}

		private void ReloadPalette(ushort address)
		{
			PenIndexPalette = new byte[16];
			for (int index = 0; index < 8; index++)
			{
				byte value = ramMemory[address + index];
				PenIndexPalette[2 * index] = (byte)((value >> 4) & 0x0F);
				PenIndexPalette[2 * index + 1] = (byte)(value & 0x0F);
			}
		}
	}
}
