namespace CluedIn.ExternalSearch.Providers.AngelList.Model
{
	public class StartupRole
	{
		public int id { get; set; }
		public string role { get; set; }
		public string created_at { get; set; }
		public object started_at { get; set; }
		public object ended_at { get; set; }
		public bool confirmed { get; set; }
		public Tagged tagged { get; set; }
		public StartupFromRole startup { get; set; }
	}
}