using System;
using System.Threading.Tasks;
using AgentCS.Services;
using AgentCS.Agents;

class Program
{
    static async Task Main(string[] args)
    {
        string dbPath = "shop.db";
        string geminiApiKey = "AIzaSyCmAkO5Z2B7lC1hpeKlAbTZme6p6sLBW5A"; 

        IDatabaseService dbService = new DatabaseService(dbPath);
        ILLMService geminiService = new GeminiService(geminiApiKey);
        IAgent agent = new AIAgent(dbService, geminiService);

        Console.WriteLine("Ask me anything about the shop database:");
        string question = Console.ReadLine();

        await agent.RunAsync(question);
    }
}
