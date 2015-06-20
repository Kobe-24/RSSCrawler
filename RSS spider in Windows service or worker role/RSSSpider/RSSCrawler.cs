using FeedAggregator.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;

namespace RSSSpider
{
    public class RSSCrawler 
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private ExpiryAgent _expiryAgent;             

        private int RefreshInterval
        {
            get { return Settings.GetSetting("RefreshIntervalInMinutes", 5) * 60 * 1000; }
        }
        public void Run()
        {
            Trace.TraceInformation("RSSSpider is running");

            try
            {
                // Create another thread to continuosly get the RSS feeds
                ThreadPool.QueueUserWorkItem(o => RefreshRSSFeeds());

                RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private void RefreshRSSFeeds()
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }
                var feeds = Settings.GetSetting("RSSFeeds").Split(';');
                foreach (var feed in feeds)
                {
                    try
                    {
                        // Read the feed
                        SyndicationFeed syndFeed = null;
                        Retry.Do(() =>
                        {
                            using (var r = XmlReader.Create(feed))
                            {
                                syndFeed = SyndicationFeed.Load(r);
                            }
                        }, TimeSpan.FromSeconds(3));

                        // Add Uncategorized when there's no category
                        AddMissingCategories(syndFeed);

                        // Store the feed items on Redis
                        Trace.TraceInformation("Processing feed items from {0}", feed);
                        Redis.Insert(syndFeed);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }

                Thread.Sleep(RefreshInterval);
            }
        }

        private void AddMissingCategories(SyndicationFeed syndFeed)
        {
            foreach (SyndicationItem item in syndFeed.Items)
            {
                if (item.Categories != null && item.Categories.Count == 0)
                {
                    item.Categories.Add(new SyndicationCategory("Uncategorized"));
                }
            }
        }

        private void LogException(Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }

        public bool OnStart()
        {
            // Set the maximum number of concurrent connections (for Azure Storage better performance)
            // http://social.msdn.microsoft.com/Forums/en-US/windowsazuredata/thread/d84ba34b-b0e0-4961-a167-bbe7618beb83
            ServicePointManager.DefaultConnectionLimit = Settings.GetSetting("DefaultConnectionLimit", 1000);

            try
            {
                _expiryAgent = new ExpiryAgent();
                _expiryAgent.Start();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Cannot start Expiry Agent: {0}", ex.Message);
            }

            Trace.TraceInformation("RSSSpider has been started");
            return true;
        }

        public void OnStop()
        {
            Trace.TraceInformation("RSSSpider is stopping");

            if (_expiryAgent != null)
            {
                _expiryAgent.Stop();
                _expiryAgent = null;
            }

            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();

            Trace.TraceInformation("RSSSpider has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
