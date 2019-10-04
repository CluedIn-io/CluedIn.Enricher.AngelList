using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class StartupFullObject
	{
		public SearchResult SearchResult { get; set; }
		public List<StartupRole> Roles { get; set; }
		public Startup Startup { get; set; }
	}
}