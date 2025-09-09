using System.Threading.Tasks;

namespace AgentCS.Services
{
    public interface ILLMService
    {
        Task<string> GetSqlQueryFromUserQuestion(string question);
    }
}
