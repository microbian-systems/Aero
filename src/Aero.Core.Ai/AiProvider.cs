namespace Aero.Core.Ai;

/// <summary>
/// Defines an enumeration for AiProvider.
/// </summary>
public enum AiProvider
{
    Anthropic,  // User's key
    DeepSeek,   // Very cheap
    Fireworks,  // Free
    Gemini,     // Free
    Groq,       // Free
    OpenAI,     // User's key
    OpenCode,   // User's key
    OpenRouter, // User's key
    Local       // Ollama / LM Studio
}