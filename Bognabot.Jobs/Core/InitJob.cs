using System.Threading.Tasks;

namespace Bognabot.Jobs.Init
{
    public abstract class InitJob
    {
        public abstract Task ExecuteAsync();
    }
}