using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using WpfVkontacteClient.Entities;

namespace WpfVkontacteClient
{
	public sealed class AsyncDownloader : IDisposable
	{
		private BackgroundWorker m_worker = null;
		private AutoResetEvent m_event = new AutoResetEvent(false);
		private readonly object m_lock = new object();
		bool isDisposed = false;
		private int m_overallPercentage;
		private int m_itemsCount;
		private int m_completed;

		public event EventHandler DownloadingComplete;
		public event EventHandler ProgressChanged;

		public AsyncDownloader()
		{
			m_worker = new BackgroundWorker();
			m_worker.WorkerReportsProgress = true;
			m_worker.WorkerSupportsCancellation = true;
			m_worker.DoWork += new DoWorkEventHandler(m_worker_DoWork);
			m_worker.ProgressChanged += new ProgressChangedEventHandler(m_worker_ProgressChanged);
			m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);
			m_overallPercentage = 0;
			m_itemsCount = 0;
			m_completed = 0;
		}

		void m_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			EventHandler handler = ProgressChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		void m_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EventHandler handler = DownloadingComplete;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		void m_worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (e.Argument != null && (e.Argument is List<ItemToLoad>) && (e.Argument as List<ItemToLoad>).Count > 0)
			{
				foreach (ItemToLoad data in (e.Argument as List<ItemToLoad>))
				{
					if (e.Cancel)
						break;
					WebClient client = new WebClient();
					client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
					client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
					data.State = LoadState.Loading;
					client.DownloadFileAsync(new Uri(data.Url), data.PathToSave, data);
					m_event.WaitOne();
				}
			}
		}

		void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Error != null || e.Cancelled)
				(e.UserState as ItemToLoad).State = LoadState.Fail;
			else
				(e.UserState as ItemToLoad).State = LoadState.Complete;

			Interlocked.Increment(ref m_completed);
			lock (m_lock)
			{
				m_overallPercentage = ((100 * m_completed) / m_itemsCount);
				m_worker.ReportProgress(m_overallPercentage);
				Monitor.Pulse(m_lock);
			}
			if (sender is WebClient)
			{
				(sender as WebClient).DownloadProgressChanged -= new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
				(sender as WebClient).DownloadFileCompleted -= new AsyncCompletedEventHandler(client_DownloadFileCompleted);
			}

			m_event.Set();
		}

		void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			(e.UserState as ItemToLoad).State = LoadState.Loading;
			(e.UserState as ItemToLoad).Percentage = e.ProgressPercentage;
		}

		private List<ItemToLoad> m_data = null;
		public List<ItemToLoad> DataToDownload
		{
			get { return m_data; }
			set
			{
				m_itemsCount = value.Count;
				m_data = value;
			}
		}

		public int OverallPercentage
		{
			get { return m_overallPercentage; }
		}

		public void Download()
		{
			m_worker.RunWorkerAsync(DataToDownload);
		}

		public void Cancel()
		{
			if (m_worker.IsBusy)
				m_worker.CancelAsync();
		}

		public void Dispose()
		{
			if (m_worker != null && !isDisposed)
			{
				isDisposed = true;
				if (m_worker.IsBusy)
					m_worker.CancelAsync();

				m_worker.DoWork -= new DoWorkEventHandler(m_worker_DoWork);
				m_worker.ProgressChanged -= new ProgressChangedEventHandler(m_worker_ProgressChanged);
				m_worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(m_worker_RunWorkerCompleted);

				m_event.Close();
				m_event.Dispose();
				m_worker.Dispose();

				GC.Collect(0);
			}
		}
	}
}
