using RSSSpider;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace WindowsService
{
    public partial class MyService : ServiceBase
    {
        private readonly RSSCrawler spider = new RSSCrawler();
        private System.Timers.Timer aTimer;

        public MyService()
        {
            InitializeComponent();
            this.AutoLog = false;
            eventLog1 = new System.Diagnostics.EventLog();


            if (!System.Diagnostics.EventLog.SourceExists("RSSSpider"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "RSSSpider", "RSSSpiderLog");
            }

            eventLog1.Source = "RSSSpider";
            eventLog1.Log = "RSSSpiderLog";
        }

        protected override void OnStart(string[] args)
        {
            spider.OnStart();

            Trace.TraceInformation("The RSS Spider has been started");
            eventLog1.WriteEntry("The RSS Spider has been started");
            // Create a timer with a ten second interval.
            aTimer = new System.Timers.Timer(10000);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = 2000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Stop();
            aTimer.Enabled = false;
            spider.Run();
        }

        protected override void OnStop()
        {
            spider.OnStop();            

            Trace.TraceInformation("The RSS Spider has stopped");
            eventLog1.WriteEntry("The RSS Spider has stopped");
        }
    }
}
