using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AdvanceBG
{
	public class Settings
	{
		private static string filename;

		public List<string> MRUList { get; set; }
		public bool ShowGrid { get; set; }
		public Color GridColor { get; set; }
		public string Username { get; set; }
		[DefaultValue(true)]
		public bool TransparentBackgroundExport { get; set; }
		public bool UseHexadecimalIndexesExport { get; set; }
		[DefaultValue("1x")]
		public string ZoomLevel { get; set; }
		public Tab CurrentTab { get; set; }
		public WindowMode WindowMode { get; set; }
		[DefaultValue(true)]
		public bool ShowMenu { get; set; }
		[DefaultValue(true)]
		public bool EnableDraggingPalette { get; set; }
		[DefaultValue(true)]
		public bool EnableDraggingTiles { get; set; }

		public static Settings Load()
		{
			filename = Path.Combine(Application.StartupPath, "AdvanceBG.json");
			if (File.Exists(filename))
				return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filename));
			else
			{
				Settings result = new Settings
				{
					MRUList = new List<string>(),
					GridColor = Color.Red,
					TransparentBackgroundExport = true,
					ZoomLevel = "1x",
					ShowMenu = true,
					EnableDraggingPalette = true,
					EnableDraggingTiles = true
				};
				return result;
			}
		}

		public void Save()
		{
			File.WriteAllText(filename, JsonConvert.SerializeObject(this));
		}
	}

	public enum Tab
	{
		Foreground,
		Art
	}

	public enum WindowMode
	{
		Normal,
		Maximized,
		Fullscreen
	}
}
