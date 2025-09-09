using System.Threading.Tasks;

namespace AgentCS.Agents
{
    public interface IAgent
    {
        Task RunAsync(string question);
    }
}
