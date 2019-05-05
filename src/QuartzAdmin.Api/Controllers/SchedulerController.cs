using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using QuartzAdmin.Api.Models;

namespace QuartzAdmin.Api.Controllers
{
    /// <summary>
    /// 调度器
    /// </summary>
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        private readonly IScheduler _scheduler;

        public SchedulerController(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            await _scheduler.Start();
            return Ok("启动成功");
        }

        [HttpPost]
        public async Task<IActionResult> Stop(string name)
        {
            await _scheduler.Shutdown(true);
            return Ok("停止成功");
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var job = JobBuilder.Create<ExampleJob>()
            .WithIdentity("job_" + DateTime.Now.ToString("yyyyMMddHHmm"))
            .RequestRecovery()
            .Build();

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("triger_"+DateTime.Now.ToString("yyyyMMddHHmm"))
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Second))
                .WithSimpleSchedule(x => x.WithRepeatCount(20).WithInterval(TimeSpan.FromMilliseconds(3000)))
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
            return Ok();
        }
    }
}
