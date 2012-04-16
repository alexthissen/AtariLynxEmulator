using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms
{
	public class Eeprom93C46A : Eeprom93XX
	{
		public Eeprom93C46A() : base(9, 7, Organization.Byte) { }
	}

	public class Eeprom93C46B : Eeprom93XX
	{
		public Eeprom93C46B() : base(8, 6, Organization.Word) { }
	}

	public class Eeprom93C56B : Eeprom93XX
	{
		public Eeprom93C56B() : base(10, 7, Organization.Word) { }
	}

	public class Eeprom93C66B : Eeprom93XX
	{
		public Eeprom93C66B() : base(10, 8, Organization.Word) { }
	}

	public class Eeprom93C76B : Eeprom93XX
	{
		public Eeprom93C76B() : base(12, 9, Organization.Word) { }
	}

	public class Eeprom93C86B : Eeprom93XX
	{
		public Eeprom93C86B() : base(12, 10, Organization.Word) { }
	}
}
