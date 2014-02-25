using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kopi
{
	public class Settings
	{
		public List<Mapping> Mappings = new List<Mapping>();
	}
	
	public class Mapping
	{
		public string Source;
		public string Destination;
        public bool IgnoreTimestamp;

		public Mapping(string a_src, string a_dst, bool a_ignoreTimestamp)
		{ 
			Source = a_src;
			Destination = a_dst;
            IgnoreTimestamp = a_ignoreTimestamp;
		}

		public override string ToString()
		{
			return String.Concat(Source, " -> ", Destination, IgnoreTimestamp ? "(ignore timestamp)" : "");
		}
	}
}
