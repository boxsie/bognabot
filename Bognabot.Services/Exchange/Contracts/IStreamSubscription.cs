using System.Threading.Tasks;

namespace Bognabot.Services.Exchange.Contracts
{
    public interface IStreamSubscription
    {
        Task TriggerUpdate(object obj);
    }
}