using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public enum QuadrantOrder
	{
		DownRight = 0,
		UpRight = 1,
		UpLeft = 2,
		DownLeft = 3
	}

	public class Quadrant
	{
		public int HorizontalIncrease;
		public int VerticalIncrease;
		public QuadrantOrder Order;

		public Quadrant(int horizontalIncrease, int verticalIncrease, QuadrantOrder order)
		{
			this.HorizontalIncrease = horizontalIncrease;
			this.VerticalIncrease = verticalIncrease;
			this.Order = order;
		}
	}
}
