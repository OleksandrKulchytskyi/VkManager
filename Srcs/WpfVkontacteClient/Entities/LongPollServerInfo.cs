namespace WpfVkontacteClient.Entities
{
	public class LongPollServerInfo
	{
		public string Key { get; set; }

		public string Server { get; set; }

		public long Ts { get; set; }

		public override string ToString()
		{
			return string.Format("{0} - {1} - {2}", Key, Server, Ts);
		}
	}
}