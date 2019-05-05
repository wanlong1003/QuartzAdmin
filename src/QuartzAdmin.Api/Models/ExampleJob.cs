using Quartz;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuartzAdmin.Api.Models
{
    public class ExampleJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[示例作业] {DateTime.Now.ToString()}");
            return Task.CompletedTask;
        }
    }
}
