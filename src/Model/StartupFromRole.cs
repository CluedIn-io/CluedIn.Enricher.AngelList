namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class StartupFromRole
	{
		public int id { get; set; }
		public bool hidden { get; set; }
		public bool community_profile { get; set; }
		public string name { get; set; }
		public string angellist_url { get; set; }
		public string logo_url { get; set; }
		public string thumb_url { get; set; }
		public int quality { get; set; }
		public string product_desc { get; set; }
		public string high_concept { get; set; }
		public int follower_count { get; set; }
		public string company_url { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
	}
}