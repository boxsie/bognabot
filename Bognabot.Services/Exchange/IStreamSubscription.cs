using System.Threading.Tasks;

namespace Bognabot.Services.Exchange
{
    public interface IStreamSubscription
    {
        Task TriggerUpdate(object obj);
    }
}