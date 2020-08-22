using Quartz;
using System;

namespace SentFileToServer
{
    public class MyJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {

            UploadFileToServer.SendToServer();

        }
    }

}
