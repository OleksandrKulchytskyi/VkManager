using System.Data;

namespace WpfVkontacteClient.Entities
{
	public class BalanceInfo
	{
		private string m_Balance;

		public string Balance
		{
			get
			{
				return m_Balance;
			}
		}

		public override string ToString()
		{
			return string.Format("Текущий баланс {0}", m_Balance);
		}

		public BalanceInfo(DataRow dr)
		{
			if (dr.Table.Columns.Contains("balance"))
			{
				this.m_Balance = dr["balance"].ToString();
			}
		}
	}
}