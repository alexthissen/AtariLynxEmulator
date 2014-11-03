using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComLynxReceiver.Disassembler
{
	public class LabelParser
	{
		public IDictionary<ushort, string> Parse(Stream stream)
		{
			IDictionary<ushort, string> labels = new Dictionary<ushort, string>();
			using (StreamReader reader = new StreamReader(stream))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					KeyValuePair<ushort, string> pair = ParseLine(line);
					if (!labels.ContainsKey(pair.Key)) labels.Add(pair);
				}
			}
			return labels;
		}

		private KeyValuePair<ushort, string> ParseLine(string line)
		{
			Regex regex = new Regex(@"^al ([0-9A-F]{6}) \.(@?\w+)$");
			Match match = regex.Match(line);
			ushort address = UInt16.Parse(match.Groups[1].Value.Substring(2), NumberStyles.HexNumber);
			string label = match.Groups[2].Value;

			KeyValuePair<ushort, string> pair = new KeyValuePair<ushort, string>(address, label);
			return pair;
		}
	}
}
