using System.Net.Http;
using System.Text;
using System.Text.Json;

var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
var chatId = "5508459447";
var geminiKey = Environment.GetEnvironmentVariable("GEMINI_KEY");

using var client = new HttpClient();

var hour = DateTime.UtcNow.Hour;

string prompt = hour < 12
    ? "Give 5 English words with Arabic meaning and 5 simple sentences"
    : hour < 18
    ? "Give short English conversation for beginner"
    : "Give a short paragraph and conversation for English learner";

// 🔥 Gemini API
var requestBody = new
{
    contents = new[]
    {
        new
        {
            parts = new[]
            {
                new { text = prompt }
            }
        }
    }
};

var json = JsonSerializer.Serialize(requestBody);

var response = await client.PostAsync(
    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={geminiKey}",
    new StringContent(json, Encoding.UTF8, "application/json")
);

var result = await response.Content.ReadAsStringAsync();

using var doc = JsonDocument.Parse(result);

string message = "Error ❌";

if (doc.RootElement.TryGetProperty("candidates", out var candidates))
{
    message = candidates[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();
}

// إرسال Telegram
var content = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("chat_id", chatId),
    new KeyValuePair<string, string>("text", message)
});

await client.PostAsync($"https://api.telegram.org/bot{telegramToken}/sendMessage", content);

Console.WriteLine("Sent with Gemini 🚀");
