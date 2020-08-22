using NLog;
using Quartz;
using Topshelf;
using Topshelf.Quartz;

namespace SentFileToServer
{
    class Program
    {
        private static readonly Logger myLogger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {

            HostFactory.Run(x =>
            {
                x.Service<FileUploadService>(s =>
                {
                    s.WhenStarted(service => service.OnStart());
                    s.WhenStopped(service => service.OnStop());
                    s.ConstructUsing(() => new FileUploadService());

                    s.ScheduleQuartzJob(q =>
                        q.WithJob(() =>
                            JobBuilder.Create<MyJob>().Build())
                            .AddTrigger(() => TriggerBuilder.Create()
                                .WithSimpleSchedule(b => b
                                    .WithIntervalInSeconds(30)
                                    .RepeatForever())
                                .Build()));
                });
                x.RunAsLocalSystem()
                    .DependsOnEventLog()
                    .StartAutomatically()
                    .EnableServiceRecovery(rc => rc.RestartService(1));

                x.SetServiceName("My Topshelf Service");
                x.SetDisplayName("My Topshelf Service");
                x.SetDescription("My Topshelf Service's description");
                x.UseNLog();
                
            });




        }
    }
}
