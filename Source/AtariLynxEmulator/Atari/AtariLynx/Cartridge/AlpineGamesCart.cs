using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	class AlpineGamesCart: RomCart
	{
		public AlpineGamesCart(): base(0, 0)
		{

		}
		protected override byte Peek(RomCartMemoryBank bank)
		{
			return base.Peek(bank);
		}
	}
}
