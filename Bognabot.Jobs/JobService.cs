using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bognabot.Jobs.Core;
using Bognabot.Jobs.Init;
using Bognabot.Jobs.Sync;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

namespace Bognabot.Jobs
{
    public class JobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobService> _logger;

        public JobService(IServiceProvider serviceProvider, ILogger<JobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            await RunInitJobs();
            await RunSyncJobs();
        }

        private async Task RunInitJobs()
        {
            var jobTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(InitJob)));

            foreach(var jt in jobTypes)
            {
                var jobInstance = (InitJob)_serviceProvider.GetService(jt);

                await jobInstance.ExecuteAsync();
            }
        }

        private async Task RunSyncJobs()
        {
            var schedFact = new StdSchedulerFactory();
            var jobFact = new JobFactory(_serviceProvider);

            var scheduler = await schedFact.GetScheduler();

            scheduler.JobFactory = jobFact;

            await scheduler.Start();

            var jobTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(SyncJob)));

            foreach (var jt in jobTypes)
            {
                var jobInstance = (SyncJob)_serviceProvider.GetService(jt);

                var job = JobBuilder.Create(jt)
                    .WithIdentity($"{jt.Name}Job")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jt.Name}Trigger")
                    .StartNow()
                    .WithSimpleSchedule(jobInstance.Schedule)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }
    }
}