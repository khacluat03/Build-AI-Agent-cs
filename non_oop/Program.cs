using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv; 
// ---------------------- Database Service ----------------------
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public List<Dictionary<string, object>> ExecuteQuery(string sql)
    {
        var results = new List<Dictionary<string, object>>();

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }
}

// ---------------------- Gemini Service ----------------------
public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetSqlQueryFromUserQuestion(string question)
    {
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new {
                    parts = new[]
                    {
                        new { text =
                    $"You are an assistant that ONLY outputs SQL code." +
                    $"Schema: Customers(id, name, email, phone), Products(id, name, price), Orders(id, customer_id, product_id, quantity)." +
                    $"Convert the following question into a valid SQLite query." +
                    $"Always use case-insensitive matching with LOWER() and LIKE instead of '=' when filtering text." +
                    $"Ensure the query is optimized for retrieving product-related data efficiently." +
                    $"Handle joins appropriately to include relevant data from Customers, Products, and Orders tables." +
                    $"Do not include explanations. Do not use code blocks. Do not add ``` anywhere." +
                    $"Return ONLY the SQL statement." +
                    $"Question: {question}" }
                    }
                }
            }
        };

        var response = await _httpClient.PostAsync(endpoint,
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API Error: {response.StatusCode} - {errorText}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        string sql = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

        return sql;
    }
}


// ---------------------- AI Agent ----------------------
public class AIAgent
{
    private readonly DatabaseService _dbService;
    private readonly GeminiService _gemini;

    public AIAgent(DatabaseService dbService, GeminiService gemini)
    {
        _dbService = dbService;
        _gemini = gemini;
    }

    public async Task RunAsync(string question)
    {
        Console.WriteLine($"User Question: {question}");

        // 1. Hỏi Gemini để lấy SQL
        string sql = await _gemini.GetSqlQueryFromUserQuestion(question);
        Console.WriteLine($"\n[AI Generated SQL]: {sql}");

        // 2. Thực thi SQL trên database
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

// ---------------------- Program ----------------------
class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string dbPath = "shop.db"; 
        string geminiApiKey = Environment.GetEnvironmentVariable("geminiApiKey");

        var dbService = new DatabaseService(dbPath);
        var geminiService = new GeminiService(geminiApiKey);
        var agent = new AIAgent(dbService, geminiService);

        Console.WriteLine("Ask me anything about the shop database:");
        string question = Console.ReadLine();

        await agent.RunAsync(question);
    }
}
