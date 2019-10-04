using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class Startup
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
		public string company_size { get; set; }
		public string high_concept { get; set; }
		public int follower_count { get; set; }
		public string company_url { get; set; }
		public string created_at { get; set; }
		public string updated_at { get; set; }
		public string crunchbase_url { get; set; }
		public string twitter_url { get; set; }
		public string facebook_url { get; set; }
		public string linkedin_url { get; set; }
		public string blog_url { get; set; }
		public string video_url { get; set; }
		public string launch_date { get; set; }
		public List<Market> markets { get; set; }
		public List<CompanyType> company_type { get; set; }
		public List<Location> locations { get; set; }
		public Status status { get; set; }
		public List<Screenshot> screenshots { get; set; }
	}
}