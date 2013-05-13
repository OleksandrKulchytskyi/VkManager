using System.Windows;
using System.Windows.Controls;

namespace WpfVkontacteClient.TemplateSelector
{
	public class MessageTemplateSelector:DataTemplateSelector
	{
		public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
		{
			if (item != null && item is Entities.UserMessage)
			{
				var msg = item as Entities.UserMessage;
				if (msg.ReadState)
					return (container as FrameworkElement).FindResource("readedTemplate") as DataTemplate;
				else
					return (container as FrameworkElement).FindResource("nonReadTemplate") as DataTemplate;
			}
			return null;
		}
	}
}
