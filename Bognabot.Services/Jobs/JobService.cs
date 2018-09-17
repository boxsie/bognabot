using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bognabot.Services.Jobs.Core;
using NLog;
using Quartz.Impl;

namespace Bognabot.Services.Jobs
{
    public class JobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public JobService(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            var schedFact = new StdSchedulerFactory();
            var jobFact = new JobFactory(_serviceProvider);

            var scheduler = await schedFact.GetScheduler();

            scheduler.JobFactory = jobFact;

            await scheduler.Start();

            var jobTypes = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(SyncJob)));

            foreach (var jt in jobTypes)
            {
                var jobInstance = (SyncJob)_serviceProvider.GetService(jt);

                var job = jobInstance.GetJob(jt);
                var trigger = jobInstance.GetTrigger();

                await scheduler.ScheduleJob(job, trigger);
            }
        }
    }
}