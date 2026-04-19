using System.Net.Http;
using System.Text;
using System.Text.Json;

// 🔐 قراءة المفاتيح
var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
var chatId = "5508459447";
var geminiKey = Environment.GetEnvironmentVariable("GEMINI_KEY");

using var client = new HttpClient();

// 🧠 تأكد من المفاتيح
if (string.IsNullOrEmpty(telegramToken))
{
    Console.WriteLine("❌ TELEGRAM TOKEN MISSING");
    return;
}

if (string.IsNullOrEmpty(geminiKey))
{
    Console.WriteLine("❌ GEMINI KEY MISSING");
    return;
}

Console.WriteLine("✅ Keys Loaded");

// ⏰ تحديد نوع المحتوى حسب الوقت
var hour = DateTime.UtcNow.Hour;

string prompt = hour < 12
    ? "Give 5 English words with Arabic meaning and 5 simple sentences"
    : hour < 18
    ? "Give short English conversation for beginner"
    : "Give a short paragraph and conversation for English learner";

// 📦 تجهيز الطلب
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

// 🚀 إرسال الطلب لـ Gemini
var response = await client.PostAsync(
    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={geminiKey}",
    new StringContent(json, Encoding.UTF8, "application/json")
);

// 🧠 طباعة الحالة
Console.WriteLine("Status Code: " + response.StatusCode);

// 📩 قراءة الرد
var result = await response.Content.ReadAsStringAsync();

Console.WriteLine("🔍 Full Gemini Response:");
Console.WriteLine(result);

// 🔍 تحليل JSON
using var doc = JsonDocument.Parse(result);

string message = "";

// 🧠 استخراج النص
if (doc.RootElement.TryGetProperty("candidates", out var candidates))
{
    try
    {
        message = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
    catch
    {
        Console.WriteLine("⚠️ Error reading Gemini response format");
        message = "⚠️ Format error from AI";
    }
}
else
{
    Console.WriteLine("❌ No candidates found");
    message = "❌ Gemini error";
}

// 🛑 fallback
if (string.IsNullOrWhiteSpace(message))
{
    message = "⚠️ Empty response from Gemini";
}

// 📤 طباعة الرسالة
Console.WriteLine("📤 Final Message:");
Console.WriteLine(message);

// 🚀 إرسال Telegram
var telegramResponse = await client.PostAsync(
    $"https://api.telegram.org/bot{telegramToken}/sendMessage",
    new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("chat_id", chatId),
        new KeyValuePair<string, string>("text", message)
    })
);

// 📩 رد Telegram
var telegramResult = await telegramResponse.Content.ReadAsStringAsync();

Console.WriteLine("📩 Telegram Response:");
Console.WriteLine(telegramResult);

Console.WriteLine("🚀 Done");
