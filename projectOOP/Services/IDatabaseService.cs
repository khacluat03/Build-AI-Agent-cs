using System.Collections.Generic;

namespace AgentCS.Services
{
    public interface IDatabaseService
    {
        List<Dictionary<string, object>> ExecuteQuery(string sql);
    }
}
