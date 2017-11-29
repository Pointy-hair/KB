namespace KnowledgeBank.Web.Settings
{
	public class MailSettings
	{
		public EmailAddress From { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
		public bool UseSsl { get; set; }
		public int RetryCount { get; set; }
	}
}