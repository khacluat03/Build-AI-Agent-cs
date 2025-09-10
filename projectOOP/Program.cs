using System;
using System.Threading.Tasks;
using AgentCS.Services;
using AgentCS.Agents;
using DotNetEnv;
class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string dbPath = "shop.db";
        string geminiApiKey = Environment.GetEnvironmentVariable("geminiApiKey");

        IDatabaseService dbService = new DatabaseService(dbPath);
        ILLMService geminiService = new GeminiService(geminiApiKey);
        IAgent agent = new AIAgent(dbService, geminiService);

        Console.WriteLine("Ask me anything about the shop database:");
        string question = Console.ReadLine();

        await agent.RunAsync(question);
    }
}
