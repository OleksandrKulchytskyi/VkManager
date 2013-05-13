using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfVkontacteClient.Controls
{
	/// <summary>
	/// Interaction logic for MyMediaControl.xaml
	/// </summary>
	public partial class MyMediaControl : UserControl
	{
		protected MediaPlayer m_player = null;
		protected bool IsFirsttime = true;
		private bool isCollapsed = false;
		protected DispatcherTimer m_timer = null;

		public MyMediaControl()
		{
			InitializeComponent();
			m_player = new MediaPlayer();
			m_timer = new DispatcherTimer();
			m_timer.Interval = new TimeSpan(0, 0, 1);
			m_timer.Tick += new EventHandler(m_timer_Tick);

			Binding volume = new Binding();
			volume.Source = m_player;
			volume.Path = new PropertyPath("Volume");
			volume.Mode = BindingMode.TwoWay;
			sldVolume.SetBinding(Slider.ValueProperty, volume);

			m_player.MediaOpened += new EventHandler(m_player_MediaOpened);
			m_player.MediaEnded += new EventHandler(m_player_MediaEnded);
		}

		void m_player_MediaEnded(object sender, EventArgs e)
		{
			m_player.Stop();
			m_timer.Stop();
		}

		void m_player_MediaOpened(object sender, EventArgs e)
		{
			sldLoop.Maximum = m_player.NaturalDuration.TimeSpan.TotalMilliseconds;
		}

		void m_timer_Tick(object sender, EventArgs e)
		{
			prgDownl.Value = m_player.DownloadProgress * 100;
			prgDownl.UpdateLayout();

			sldLoop.Value = m_player.Position.Milliseconds;
			sldLoop.UpdateLayout();

			this.changeStatus();
		}

		public bool AudioPlaying
		{
			get { return (bool)GetValue(AudioPlayingProperty); }
			set { SetValue(AudioPlayingProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AudioPlaying.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AudioPlayingProperty =
			DependencyProperty.Register("AudioPlaying", typeof(bool), typeof(MyMediaControl), new UIPropertyMetadata(false));


		public bool IsPause
		{
			get { return (bool)GetValue(IsPauseProperty); }
			set { SetValue(IsPauseProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsPause.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsPauseProperty =
			DependencyProperty.Register("IsPause", typeof(bool), typeof(MyMediaControl), new UIPropertyMetadata(false));


		public string PlayerSoyrce
		{
			get { return (string)GetValue(PlayerSoyrceProperty); }
			set { SetValue(PlayerSoyrceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PlayerSoyrce.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PlayerSoyrceProperty =
			DependencyProperty.Register("PlayerSoyrce", typeof(string), typeof(MyMediaControl), new UIPropertyMetadata(string.Empty));


		public long FromId
		{
			get { return (long)GetValue(FromIdProperty); }
			set { SetValue(FromIdProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FromId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FromIdProperty =
			DependencyProperty.Register("FromId", typeof(long), typeof(MyMediaControl));


		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			if (IsPause)
			{
				m_player.Play();
				IsPause = false;
				return;
			}

			if (IsFirsttime || isCollapsed)
			{
				var audios = VKontakteApiWrapper.Instance.AudioGetById(new List<string>(){ string.Format("{0}_{1}",
													(this.DataContext as Entities.UserAudio).OwnerId,
													(this.DataContext as Entities.UserAudio).AudioId)
													});
				if (audios != null && audios[0] != null)
					PlayerSoyrce = audios[0].Url;

				if (!string.IsNullOrEmpty(PlayerSoyrce))
				{
					m_timer.Start();
					m_player.Open(new Uri(PlayerSoyrce, UriKind.RelativeOrAbsolute));
					m_player.Play();
					AudioPlaying = true;
					IsFirsttime = false;
					return;
				}
			}

			m_player.Play();
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (m_player.HasAudio && AudioPlaying)
			{
				m_player.Stop();
				AudioPlaying = false;
				m_player.Close();
				m_timer.Stop();
			}
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			if (m_player.HasAudio && m_player.CanPause)
			{
				m_player.Pause();
				IsPause = true;
			}
		}

		private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			m_player.Volume = sldVolume.Value;
		}

		void changeStatus()
		{
			if (AudioPlaying)
			{
				string sec, min, hours;

				#region customizeTime
				if (m_player.Position.Seconds < 10)
					sec = "0" + m_player.Position.Seconds.ToString();
				else
					sec = m_player.Position.Seconds.ToString();

				if (m_player.Position.Minutes < 10)
					min = "0" + m_player.Position.Minutes.ToString();
				else
					min = m_player.Position.Minutes.ToString();

				if (m_player.Position.Hours < 10)
					hours = "0" + m_player.Position.Hours.ToString();
				else
					hours = m_player.Position.Hours.ToString();

				#endregion customizeTime

				if (m_player.Position.Hours == 0)
				{
					txtSec.Text = string.Format("{0}:{1}", min, sec); ;
				}
				else
				{
					txtSec.Text = string.Format("{0}:{1}:{2}", hours, min, sec); ;
				}
			}
		}

		public void StopAllPlaying()
		{
			if (m_player.HasAudio && AudioPlaying)
			{
				m_player.Stop();
				m_player.Close();
				if (m_timer.IsEnabled)
					m_timer.Stop();

				isCollapsed = true;
			}
		}
	}
}
