using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AstroComputationEngine.Models.AI;

public class AIResponse
{
    public string Id { get; set; }
    public string Provider { get; set; }
    public string Model { get; set; }

    [JsonPropertyName("_object")]
    public string Object { get; set; }

    public int Created { get; set; }
    public Choice[] Choices { get; set; }
    public Usage Usage { get; set; }
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public object PromptTokensDetails { get; set; }
}

public class Choice
{
    public object Logprobs { get; set; }
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
    [JsonPropertyName("native_finish_reason")]
    public string NativeFinishReason { get; set; }
    public int Index { get; set; }
    public Message Message { get; set; }
}

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
    public object Refusal { get; set; }
    public object Reasoning { get; set; }
}

