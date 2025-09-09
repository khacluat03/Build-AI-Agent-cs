using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentCS.Services
{
    public class GeminiService : ILLMService
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
}
