using System.ComponentModel;

namespace WpfVkontacteClient
{
	public enum LoadState
	{
		None = 0,
		Loading,
		Complete,
		Fail
	}

	namespace Entities
	{
		public class ItemToLoad : INotifyPropertyChanged
		{
			private LoadState state;

			public LoadState State
			{
				get { return state; }
				set { state = value; OnPropertyChanged("State"); }
			}

			private int perc;

			public int Percentage
			{
				get { return perc; }
				set
				{
					perc = value;
					OnPropertyChanged("Percentage");
				}
			}

			private string url;

			public string Url
			{
				get { return url; }
				set
				{
					url = value;
					OnPropertyChanged("Url");
				}
			}

			private string path;

			public string PathToSave
			{
				get { return path; }
				set { path = value; OnPropertyChanged(PathToSave); }
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected void OnPropertyChanged(string propName)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propName));
				}
			}
		}
	}
}