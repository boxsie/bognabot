using System.Threading.Tasks;

namespace Bognabot.Jobs.Core
{
    public interface IFaFJob
    {
        Task ExecuteAsync();
    }
}