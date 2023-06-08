using Newtonsoft.Json;

namespace AdvanceTools
{
	public class GameInfo
	{
		[JsonProperty("game")]
		public int Game { get; set; }
		[JsonProperty("code")]
		public string Code { get; set; }
		[JsonProperty("versions")]
		public VersionInfo[] Versions { get; set; }
		[JsonProperty("levels")]
		public string[] LevelNames { get; set; }
		[JsonProperty("backgrounds")]
		public BackgroundInfo[] Backgrounds { get; set; }
		[JsonProperty("animationCount")]
		public int AnimationCount { get; set; }

		public static GameInfo[] Read(string filename) => JsonConvert.DeserializeObject<GameInfo[]>(System.IO.File.ReadAllText(filename));
	}

	public class VersionInfo
	{
		[JsonProperty("region")]
		public char Region { get; set; }
		[JsonProperty("version")]
		public byte Version { get; set; }
		[JsonProperty("mapTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int MapTable { get; set; }
		[JsonProperty("collisionTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int CollisionTable { get; set; }
		[JsonProperty("interactableTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int InteractableTable { get; set; }
		[JsonProperty("itemTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int ItemTable { get; set; }
		[JsonProperty("enemyTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int EnemyTable { get; set; }
		[JsonProperty("ringTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int RingTable { get; set; }
		[JsonProperty("startTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int StartTable { get; set; }
		[JsonProperty("spriteTable")]
		[JsonConverter(typeof(IntHexConverter))]
		public int SpriteTable { get; set; }
	}

	public class BackgroundInfo
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("mode")]
		public BGMode Mode { get; set; }
	}

	public class IntHexConverter : JsonConverter<int>
	{
		public override int ReadJson(JsonReader reader, System.Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer) => int.Parse((string)reader.Value, System.Globalization.NumberStyles.HexNumber);
		public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer) => writer.WriteValue(value.ToString("X"));
	}
}
