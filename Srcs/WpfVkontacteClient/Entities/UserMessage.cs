using System;
using System.Data;
using Utils;

namespace WpfVkontacteClient.Entities
{
	public class UserMessage
	{
		public UserMessage(DataRow row)
		{
			if (row.Table.Columns.Contains("mid"))
				long.TryParse(row["mid"].ToString(), out this._messageId);

			if (row.Table.Columns.Contains("uid"))
				long.TryParse(row["uid"].ToString(), out this._uid);

			if (row.Table.Columns.Contains("body"))
				this._body = row["body"].ToString().EscapeXmlString();

			if (row.Table.Columns.Contains("title"))
				this._title = row["title"].ToString();

			if (row.Table.Columns.Contains("date"))
				this._date = row["date"].ToString();

			if (row.Table.Columns.Contains("read_state"))
				this._state = Extension.FromStringToBool(row["read_state"].ToString());

			if (row.Table.Columns.Contains("out"))
				this._out = row["out"].ToString();
		}

		public UserMessage(DataRow rowMsg, DataRow attachment)
			: this(rowMsg)
		{
			if (attachment != null)
			{
				HasAttachment = true;
				AttachmentType = attachment[0] as string;
			}
		}

		private string _body;
		public string MessageBody
		{
			get
			{
				return _body;
			}
		}

		private string _title;
		public string MessageTitle
		{
			get
			{
				return _title;
			}
		}

		private long _messageId;
		public long MessageId
		{
			get
			{
				return _messageId;
			}
		}

		private long _uid;

		public long UserId
		{
			get
			{
				return _uid;
			}
		}

		private bool _state;

		public bool ReadState
		{
			get
			{
				return _state;
			}
			set { _state = value; }
		}

		private string _date;
		public DateTime Date
		{
			get
			{
				return Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(_date));
			}
		}

		private string _out;
		public string Out
		{
			get { return _out; }
		}

		private bool _hasAttachment;
		public bool HasAttachment
		{
			get { return _hasAttachment; }
			set { _hasAttachment = value; }
		}

		private string _attachmentType;
		public string AttachmentType
		{
			get { return _attachmentType; }
			set { _attachmentType = value; }
		}


		public override string ToString()
		{
			return string.Format("{0} \r\n {1}", _title, _body);
		}
	}
}