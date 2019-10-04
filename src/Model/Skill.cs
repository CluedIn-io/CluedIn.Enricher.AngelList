using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class Skill
	{

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("tag_type")]
		public string TagType { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("display_name")]
		public string DisplayName { get; set; }

		[JsonProperty("angellist_url")]
		public string AngellistUrl { get; set; }

		[JsonProperty("level")]
		public double Level { get; set; }
	}
}