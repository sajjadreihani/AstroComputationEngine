using AstroComputationEngine.Interfaces;
using AstroComputationEngine.Models.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstroComputationEngine.Services;
public class AIService : IAIService
{
    private readonly JsonSerializerOptions deserializeOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<AIResponse> AskAI(string chart, string model = "openai/gpt-oss-20b:free")
    {
        // TODO: Move API key to secure configuration (appsettings.json, environment variables, or user secrets)
        // For now, replace this with your own OpenRouter API key
        string apiKey = "YOUR_OPENROUTER_API_KEY_HERE";

        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromMinutes(5),
        };

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are an AI assistant specialized in astronomical chart analysis using tropical zodiac conventions. Focus on: \n- Key aspects (transit-to-natal, no wider than 3° orb)\n- Active houses and emotional energy\n- Psychological insights (not predictions)\n- Use precise terms: signs, degrees, aspects.\n\n Format for mobile: bullet breaks, no markdown."
                },
                new
                {
                    role = "user",
                    content = chart
                },
                new
                {
                    role = "user",
                    content = "Analyze this chart."
                }
            }
        };

        string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

        string responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AIResponse>(responseString, deserializeOption);
    }
}
