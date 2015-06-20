using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using RSSSpider;
using System.Timers;

namespace RSSParser
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly RSSCrawler spider = new RSSCrawler();        

        public override void Run()
        {
            spider.Run();            
        }
            
        public override bool OnStart()
        {
            spider.OnStart();

            bool result = base.OnStart();
            Trace.TraceInformation("WorkerRole has been started");            

            return result;
        }        

        public override void OnStop()
        {
            spider.OnStop();
            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }        
    }
}
