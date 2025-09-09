using System;
using System.Threading.Tasks;
using AgentCS.Services;

namespace AgentCS.Agents
{
    public class AIAgent : IAgent
    {
        private readonly IDatabaseService _dbService;
        private readonly ILLMService _llmService;

        public AIAgent(IDatabaseService dbService, ILLMService llmService)
        {
            _dbService = dbService;
            _llmService = llmService;
        }

        public async Task RunAsync(string question)
        {
            Console.WriteLine($"User Question: {question}");

            string sql = await _llmService.GetSqlQueryFromUserQuestion(question);
            Console.WriteLine($"\n[AI Generated SQL]: {sql}");

            try
            {
                var results = _dbService.ExecuteQuery(sql);

                Console.WriteLine("\n[Query Results]:");
                foreach (var row in results)
                {
                    foreach (var kv in row)
                    {
                        Console.Write($"{kv.Key}: {kv.Value} | ");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error executing SQL]: {ex.Message}");
            }
        }
    }
}
