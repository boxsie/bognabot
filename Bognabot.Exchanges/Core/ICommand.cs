using System.Collections.Generic;

namespace Bognabot.Exchanges.Core
{
    public interface ICommand
    {
        IDictionary<string, string> GetRequestParams(CommandRequest request);
    }
}