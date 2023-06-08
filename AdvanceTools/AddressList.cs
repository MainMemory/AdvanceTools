using System.Collections.Generic;

namespace AdvanceTools
{
	public class AddressList<T> : List<T>
	{
		public int Address { get; set; }

		public AddressList(int address) : base()
		{
			Address = address;
		}

		public AddressList(int address, int capacity) : base(capacity)
		{
			Address = address;
		}

		public AddressList(int address, IEnumerable<T> collection) : base(collection)
		{
			Address = address;
		}
	}
}
