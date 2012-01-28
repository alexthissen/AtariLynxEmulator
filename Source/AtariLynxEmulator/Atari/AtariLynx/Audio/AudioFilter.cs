using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Atari.Lynx;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class AudioFilter
	{
		public const int AUDIO_SAMPLE_FREQ = 22050;
		public const int AUDIO_BUFFER_SIZE = AUDIO_SAMPLE_FREQ / 4;
		public const int AUDIO_SAMPLE_PERIOD = LynxHandheld.SYSTEM_FREQ / AUDIO_SAMPLE_FREQ;

		private ulong audioLastUpdateCycle = 0;
		private int audioBufferIndex = 0;

		public byte[] Buffer { get; private set; }

		public event EventHandler<BufferEventArgs> BufferReady;

		private void OnBufferReady()
		{
			if (BufferReady != null)
			{
				BufferReady(this, new BufferEventArgs() { Buffer = this.Buffer });
			}
		}

		public AudioFilter()
		{
			// Buffer must contain 16 bit PCM data
			Buffer = new byte[AUDIO_BUFFER_SIZE * 2];
		}

		// "The 4 audio channels are mixed digitally and a pulse width modulated waveform is 
		// output from Mikey to the audio filter. This filter is a 1 pole low pass fitter with a 
		// frequency cutoff at 4 KHz. The output of the filter is amplified to drive an 8 ohm speaker."
		public void Output(ulong cycleCount, byte sample)
		{
			for (; audioLastUpdateCycle + AUDIO_SAMPLE_PERIOD < cycleCount; audioLastUpdateCycle += AUDIO_SAMPLE_PERIOD)
			{
				// Output audio sample
				Buffer[audioBufferIndex++] = 0;
				Buffer[audioBufferIndex++] = sample;
				if (audioBufferIndex >= Buffer.Length)
				{
					OnBufferReady();
					audioBufferIndex = 0;
				}
			}
		}
	}
}