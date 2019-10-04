using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class Roles
	{
		public List<StartupRole> startup_roles { get; set; }
		public int total { get; set; }
		public int per_page { get; set; }
		public int page { get; set; }
		public int last_page { get; set; }
	}
}