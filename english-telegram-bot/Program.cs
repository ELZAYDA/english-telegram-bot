using System.Net.Http;
using System.Text;
using System.Text.Json;

var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
var chatId = "5508459447";
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");

using var client = new HttpClient();

var hour = DateTime.UtcNow.Hour;

string prompt = hour < 12
    ? "Give 5 English words with Arabic meaning and 5 simple sentences"
    : hour < 18
    ? "Give short English conversation for beginner"
    : "Give a short paragraph and conversation for English learner";

var requestBody = new
{
    model = "gpt-4o-mini",
    messages = new[]
    {
        new { role = "user", content = prompt }
    }
};

var requestJson = JsonSerializer.Serialize(requestBody);

var aiRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
aiRequest.Headers.Add("Authorization", $"Bearer {openAiKey}");
aiRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

var aiResponse = await client.SendAsync(aiRequest);
var aiContent = await aiResponse.Content.ReadAsStringAsync();

using var doc = JsonDocument.Parse(aiContent);
var message = doc.RootElement
    .GetProperty("choices")[0]
    .GetProperty("message")
    .GetProperty("content")
    .GetString();

var telegramContent = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("chat_id", chatId),
    new KeyValuePair<string, string>("text", message)
});

await client.PostAsync($"https://api.telegram.org/bot{telegramToken}/sendMessage", telegramContent);

Console.WriteLine("Sent ✅");
