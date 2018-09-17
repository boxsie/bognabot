using System;
using System.Threading.Tasks;

namespace Bognabot.Services.Exchange
{
    public class StreamSubscription<T> : IStreamSubscription
    {
        public T[] Latest { get; set; }

        private readonly Func<T[], Task> _onUpdate;

        public StreamSubscription(Func<T[], Task> onUpdate)
        {
            _onUpdate = onUpdate;
        }

        public Task TriggerUpdate(object obj)
        {
            Latest = (T[])obj;

            return _onUpdate.Invoke(Latest);
        }
    }
}