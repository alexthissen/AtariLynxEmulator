using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public class Eeprom93C46A : Eeprom93XX
	{
		public Eeprom93C46A() : base(7, Organization.Byte) { }
	}

	public class Eeprom93C46B : Eeprom93XX
	{
		public Eeprom93C46B() : base(6, Organization.Word) { }
	}
}
