using System;

namespace KillerApps.Emulation.Eeproms
{
	public class Clock
	{
		public event EventHandler<EdgeChangedEventArgs> EdgeChange;
		private bool currentValue;

		public bool Value
		{
			get
			{
				return currentValue;
			}
			internal set
			{
				if (currentValue != value)
				{
					VoltageEdge edge = !currentValue && value ? VoltageEdge.Positive : VoltageEdge.Negative;
					OnEdgeChange(new EdgeChangedEventArgs(edge));
				}
				currentValue = value;
			}
		}

		protected virtual void OnEdgeChange(EdgeChangedEventArgs args)
		{
			if (EdgeChange != null)
				EdgeChange(this, args);
		}
	}
}
