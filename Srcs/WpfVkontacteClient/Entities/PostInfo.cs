using System;
using System.Collections.Generic;
using System.Xml;

namespace WpfVkontacteClient.Entities
{
	public class PostInfo
	{
		public PostInfo()
		{
			Comments = new List<UserComment>();
			Attachment = null;
		}

		public long Id { get; set; }

		public long FromUid { get; set; }

		public long ToUid { get; set; }

		public DateTime Date { get; set; }

		public string Text { get; set; }

		public VkMedia Media { get; set; }

		public AttachmentInfo Attachment { get; set; }

		public List<UserComment> Comments { get; set; }

		public VkPostSource PostSource { get; set; }

		public int LikesCount { get; set; }

		public int CommentsCount { get; set; }

		public bool IsOnline { get; set; }

		public int ReplyCount { get; set; }

		public bool ContainsMedia { get { return Media == null; } }

		public bool ContainsAttachment { get { return Attachment == null; } }

		public virtual void ParseXml(XmlNode node)
		{
			Id = long.Parse(node.SelectSingleNode("id/text()").Value);
			FromUid = long.Parse(node.SelectSingleNode("from_id/text()").Value);
			ToUid = long.Parse(node.SelectSingleNode("to_id/text()").Value);
			Date = Utils.DateTimeUtils.ConvertFromUnixTimestamp(double.Parse(node.SelectSingleNode("date/text()").Value));
			if (node.SelectSingleNode("text").InnerText.Length > 0)
			{
				Text = node.SelectSingleNode("text/text()").Value;
			}

			if (node.HasChildNodes)
			{
				var mediaNode = node.SelectSingleNode("media");
				if (mediaNode != null)
				{
					Media = new VkMedia();
					Media.Type = mediaNode.SelectSingleNode("type/text()").Value;

					if (mediaNode.SelectSingleNode("item_id") != null)
						Media.ItemId = long.Parse(mediaNode.SelectSingleNode("item_id/text()").Value);

					if (mediaNode.SelectSingleNode("owner_id") != null)
						Media.OwnerId = long.Parse(mediaNode.SelectSingleNode("owner_id/text()").Value);

					if (mediaNode.SelectSingleNode("share_title") != null &&
						mediaNode.SelectSingleNode("share_title").Value != null)
						Media.ShareTitle = mediaNode.SelectSingleNode("share_title/text()").Value;

					if (mediaNode.SelectSingleNode("share_url") != null)
						Media.ShareUrl = mediaNode.SelectSingleNode("share_url/text()").Value;

					if (mediaNode.SelectSingleNode("thumb_src") != null)
						Media.ThumbSource = mediaNode.SelectSingleNode("thumb_src/text()").Value;
				}

				var attachmentNode = node.SelectSingleNode("attachment");
				if (attachmentNode != null)
				{
					Attachment = new AttachmentInfo();
					Attachment.Type = attachmentNode.SelectSingleNode("type/text()").Value;

					switch (Attachment.Type.ToLower().Trim())
					{
						case "audio":
							Attachment.Audio = new UserAudio(long.Parse(attachmentNode.SelectSingleNode("audio/aid/text()").Value),
								long.Parse(attachmentNode.SelectSingleNode("audio/owner_id/text()").Value),
								attachmentNode.SelectSingleNode("audio/title/text()").Value,
								long.Parse(attachmentNode.SelectSingleNode("audio/duration/text()").Value));
							break;

						case "video":
							Attachment.Video = new UserVideo(long.Parse(attachmentNode.SelectSingleNode("video/vid/text()").Value),
								long.Parse(attachmentNode.SelectSingleNode("video/owner_id/text()").Value),
								attachmentNode.SelectSingleNode("video/title").Value == null ? string.Empty : attachmentNode.SelectSingleNode("video/title").Value,
								long.Parse(attachmentNode.SelectSingleNode("video/duration/text()").Value),
								attachmentNode.SelectSingleNode("video/image/text()").Value);
							break;

						case "share":
							Attachment.Link = new VkLink()
							{
								Url = attachmentNode.SelectSingleNode("link/url/text()").Value,
								Title = attachmentNode.SelectSingleNode("link/title/text()").Value,
								Description = attachmentNode.SelectSingleNode("link/description/text()").Value,
								ImageSource = attachmentNode.SelectSingleNode("link/image_src/text()").Value
							};
							break;

						case "note":
							Attachment.Note = new UserNote()
							{
								NoteId = long.Parse(attachmentNode.SelectSingleNode("note/nid/text()").Value),
								UserId = long.Parse(attachmentNode.SelectSingleNode("note/owner_id/text()").Value),
								Title = attachmentNode.SelectSingleNode("note/title/text()").Value,
								CommentsCount = int.Parse(attachmentNode.SelectSingleNode("note/ncom/text()").Value)
							};
							break;

						case "photo":
							Attachment.Photo = new UserPhoto(long.Parse(attachmentNode.SelectSingleNode("photo/pid/text()").Value),
								long.Parse(attachmentNode.SelectSingleNode("photo/owner_id/text()").Value),
								attachmentNode.SelectSingleNode("photo/src/text()").Value,
								attachmentNode.SelectSingleNode("photo/src_big/text()").Value);
							break;

						case "graffiti":
							Attachment.Graffiti = new UserGraffiti()
							{
								Gid = long.Parse(attachmentNode.SelectSingleNode("graffiti/gid/text()").Value),
								OwnerId = long.Parse(attachmentNode.SelectSingleNode("graffiti/owner_id/text()").Value),
								Source = attachmentNode.SelectSingleNode("graffiti/src/text()").Value,
								SourceBig = attachmentNode.SelectSingleNode("graffiti/src_big/text()").Value
							};
							break;

						default:
							break;
					}
				}

				var onlineNode = node.SelectSingleNode("online");
				if (onlineNode != null && onlineNode.Value != null)
				{
					IsOnline = onlineNode.Value == "1";
				}

				var postNode = node.SelectSingleNode("post_source");
				if (postNode != null)
				{
					PostSource = new VkPostSource() { Type = postNode.SelectSingleNode("type/text()").Value };
				}

				var likeNode = node.SelectSingleNode("likes");
				if (likeNode != null && likeNode.SelectSingleNode("count") != null)
				{
					LikesCount = Convert.ToInt32(likeNode.SelectSingleNode("count/text()").Value);
				}

				var commentNode = node.SelectSingleNode("comments");
				if (commentNode != null && commentNode.SelectSingleNode("count") != null)
				{
					CommentsCount = Convert.ToInt32(commentNode.SelectSingleNode("count/text()").Value);
				}
			}
		}
	}

	public class AttachmentInfo
	{
		public string Type { get; set; }

		public VkLink Link { get; set; }

		public UserAudio Audio { get; set; }

		public UserVideo Video { get; set; }

		public UserPhoto Photo { get; set; }

		public UserNote Note { get; set; }

		public UserGraffiti Graffiti { get; set; }
	}

	public class VkMedia
	{
		public string Type { get; set; }

		public long OwnerId { get; set; }

		public long ItemId { get; set; }

		public string ShareTitle { get; set; }

		public string ShareUrl { get; set; }

		public string ThumbSource { get; set; }
	}

	public class VkLink
	{
		public string Url { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string ImageSource { get; set; }
	}

	public class VkPostSource
	{
		public string Type { get; set; }
	}
}