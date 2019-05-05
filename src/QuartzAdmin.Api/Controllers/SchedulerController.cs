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
        private readonly ISchedulerFactory _factory;
        public SchedulerController(IScheduler scheduler, ISchedulerFactory factory)
        {
            _factory = factory;
            _scheduler = scheduler;
        }

        [HttpGet]
        public async Task<IEnumerable<IScheduler>> Gets()
        {
           return await _factory.GetAllSchedulers();
        }

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            await _scheduler.Start();
            return Ok("启动成功");
        }

        [HttpPost]
        public async Task<IActionResult> Stop()
        {
            await _scheduler.Shutdown(true);
            return Ok("停止成功");
        }

        [HttpGet]
        public  string Status()
        {
           return _scheduler.IsStarted ? "Runing" : "Stop";
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var job = JobBuilder.Create<ExampleJob>()
            .WithIdentity("job_" + DateTime.Now.ToString("yyyyMMddHHmmss"))
            .RequestRecovery()
            .Build();

            var trigger = (ISimpleTrigger)TriggerBuilder.Create()
                .WithIdentity("triger_"+DateTime.Now.ToString("yyyyMMddHHmmss"))
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Second))
                .WithSimpleSchedule(x => x.WithRepeatCount(20).WithInterval(TimeSpan.FromMilliseconds(3000)))
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
            return Ok();
        }
    }
}
