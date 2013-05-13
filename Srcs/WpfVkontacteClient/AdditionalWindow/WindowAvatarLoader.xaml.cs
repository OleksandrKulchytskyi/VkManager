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
using System.Windows.Shapes;
using System.Net;
using System.Collections.ObjectModel;
using LogModule;

namespace WpfVkontacteClient.AdditionalWindow
{
    /// <summary>
    /// Interaction logic for WindowAvatarLoader.xaml
    /// </summary>
    public partial class WindowAvatarLoader : Window
    {
        private WebClient m_webClient;
        private int m_countDownloaded = 0;
        private System.ComponentModel.BackgroundWorker m_worker;

        public Dictionary<long, string> FilesToDownload
        {
            get;
            protected set;
        }

        public WindowAvatarLoader()
        {
            InitializeComponent();
            m_webClient = new WebClient();
            this.Closing += new System.ComponentModel.CancelEventHandler(WindowAvatarLoader_Closing);
        }

        public WindowAvatarLoader(Dictionary<long, string> data)
            : this()
        {
            FilesToDownload = data;
            prgDownloadsOverall.Maximum = FilesToDownload.Count;
        }

        void WindowAvatarLoader_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_webClient != null)
                m_webClient.Dispose();

            if (m_worker != null)
            {
                m_worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(m_worker_DoWork);
                m_worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
                m_worker.ProgressChanged -= new System.ComponentModel.ProgressChangedEventHandler(m_worker_ProgressChanged);
                m_worker.Dispose();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (FilesToDownload.Count > 0)
            {
                m_worker = new System.ComponentModel.BackgroundWorker();
                m_worker.WorkerReportsProgress = true;
                m_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(m_worker_DoWork);
                m_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
                m_worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(m_worker_ProgressChanged);
                prgDownloadsOverall.Maximum = 100;
                m_worker.RunWorkerAsync(FilesToDownload);
            }
        }

        void m_worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    prgDownloadsOverall.Value = e.ProgressPercentage;
                }));
        }

        void m_worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtProgress.Text = "Загрузка завершена";
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1).Milliseconds);
                    this.Close();
                }));
            LoggingModule.Instance.WriteMessage(LoggingModule.Severity.Information, "Инициализация фотографий для кеша друзей завершена");
        }

        void m_worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            foreach (long key in (e.Argument as Dictionary<long, string>).Keys)
            {
                byte[] data = null;
                try
                {
                    data = m_webClient.DownloadData(FilesToDownload[key]);
                }
                catch { continue; }
                Utils.FriendsCache.Instance.UpdateFriendsPhotoTable(key, data, null);
                System.Threading.Interlocked.Increment(ref m_countDownloaded);
                m_worker.ReportProgress((100 * m_countDownloaded) / FilesToDownload.Count);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtProgress.Text = string.Format("Обработано записей {0}", m_countDownloaded);
                }));
            }
        }
    }
}
