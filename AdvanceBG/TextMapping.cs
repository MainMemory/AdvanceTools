using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdvanceBG
{
	public class TextMapping
	{
		public TextMapping() { }

		public TextMapping(string map)
		{
			Height = 1;
			DefaultWidth = 1;
			Characters = new Dictionary<char, CharMapInfo>();
			int i = 0;
			ushort tile = 0;
			while (i < map.Length)
			{
				char start = map[i++];
				char? end = null;
				if (i < map.Length && map[i] == '-')
				{
					++i;
					end = map[i++];
				}
				if (i < map.Length && map[i] == ':')
				{
					++i;
					int pipe = map.IndexOf('|', i);
					string num;
					if (pipe == -1)
						num = map.Substring(i);
					else
						num = map.Substring(i, pipe - i);
					i += num.Length;
					if (num.StartsWith("0x"))
						tile = ushort.Parse(num.Substring(2), NumberStyles.HexNumber);
					else
						tile = ushort.Parse(num, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
				}
				if (end.HasValue)
					for (char c = start; c <= end.Value; c++)
						Characters[c] = new CharMapInfo() { Map = new ushort[1, 1] { { tile++ } } };
				else
					Characters[start] = new CharMapInfo() { Map = new ushort[1, 1] { { tile++ } } };
				if (i < map.Length && map[i] != '|')
					throw new FormatException("Invalid text mapping string.");
				++i;
			}
		}

		public int Height { get; set; }
		public int DefaultWidth { get; set; }
		public Dictionary<char, CharMapInfo> Characters { get; set; }
	}

	public class CharMapInfo
	{
		public int? Width { get; set; }
		[JsonIgnore]
		public ushort[,] Map { get; set; }
		[JsonProperty("Map")]
		public string[][] MapText
		{
			get
			{
				string[][] result = new string[Map.GetLength(1)][];
				for (int y = 0; y < Map.GetLength(1); y++)
				{
					string[] tmp = new string[Map.GetLength(0)];
					for (int x = 0; x < Map.GetLength(0); x++)
						tmp[x] = $"0x{Map[x, y]:X}";
					result[y] = tmp;
				}
				return result;
			}
			set
			{
				Map = new ushort[value[0].Length, value.Length];
				for (int y = 0; y < Map.GetLength(1); y++)
				{
					for (int x = 0; x < Map.GetLength(0); x++)
						if (value[y][x].StartsWith("0x"))
							Map[x, y] = ushort.Parse(value[y][x].Substring(2), NumberStyles.HexNumber);
						else
							Map[x, y] = ushort.Parse(value[y][x], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
				}
			}
		}
	}
}