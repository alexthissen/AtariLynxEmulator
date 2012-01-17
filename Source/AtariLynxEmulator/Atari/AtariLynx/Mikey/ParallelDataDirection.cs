using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public enum DataDirection
	{
		Input = 0,
		Output = 1 
	}

	public class ParallelDataDirection
	{
		public ParallelDataDirection(byte initialValue)
		{
			this.ByteData = initialValue;
		}

		public byte ByteData { internal get; set; }

		public DataDirection Rest { get { return GetDirection(ParallelData.RestMask); } }
		public DataDirection NoExpansion { get { return GetDirection(ParallelData.NoExpansionMask); } }
		public DataDirection AuxiliaryDigitalInOut { get { return GetDirection(ParallelData.AuxiliaryDigitalInOutMask); } }
		public DataDirection CartAddressData { get { return GetDirection(ParallelData.CartAddressDataMask); } }
		public DataDirection ExternalPower { get { return GetDirection(ParallelData.ExternalPowerMask); } }

		private DataDirection GetDirection(byte mask)
		{
			return ((ByteData & mask) == mask) ? DataDirection.Output : DataDirection.Input; 
		}
	}
}
