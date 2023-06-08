using System.Collections.Generic;

namespace AdvanceTools
{
	public class ObjectDefinition
	{
		public string Name { get; set; }
		public List<ObjectVariant> Variants { get; set; }
	}

	public class ObjectVariant
	{
		public List<int> Levels { get; set; }
		public byte? Data1 { get; set; }
		public byte? Data2 { get; set; }
		public byte? Data3 { get; set; }
		public byte? Data4 { get; set; }
		public byte? Data5 { get; set; }
		public List<ObjectSprite> Sprites { get; set; }
	}

	public class ObjectSprite
	{
		public int Animation { get; set; }
		public int Variant { get; set; }
		public int Frame { get; set; }
		public bool XFlip { get; set; }
		public bool YFlip { get; set; }

		public ObjectSprite Clone() => (ObjectSprite)MemberwiseClone();
	}
}
