using System;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace Bognabot.Services.Jobs.Core
{
    [DisallowConcurrentExecution]
    public abstract class SyncJob : IJob
    {
        public string Name => GetType().Name;

        public Action<SimpleScheduleBuilder> Schedule => x => x.WithIntervalInSeconds(_intervalSeconds).RepeatForever();

        protected readonly ILogger Logger;

        private readonly int _intervalSeconds;
        private readonly DateTime? _startTime;

        protected abstract Task ExecuteAsync();

        protected SyncJob(ILogger logger, int intervalSeconds, DateTime? startTime = null)
        {
            Logger = logger;
            _intervalSeconds = intervalSeconds == 0 ? 30 : intervalSeconds;
            _startTime = startTime;
        }

        public IJobDetail GetJob(Type jobType)
        {
            return JobBuilder.Create(jobType)
                .WithIdentity($"{Name}_Job")
                .Build();
        }

        public ITrigger GetTrigger()
        {
            return TriggerBuilder.Create()
                .WithIdentity($"{Name}_Trigger")
                .StartAt(_startTime ?? DateTime.Now)
                .WithSimpleSchedule(Schedule)
                .Build();
        }

        public virtual async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"{ex.Message}\r\n{ex.InnerException}");
                Logger.Log(LogLevel.Error, string.Join("\r\n", ex.StackTrace));
            }
        }

        protected static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }
    }
}
