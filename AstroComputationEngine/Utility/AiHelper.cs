using AstroComputationEngine.Models.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Utility;

public static class AiHelper
{
    public static readonly List<AiModel> Models = [
                new() { Name = "Chat GPT", Id = "openai/gpt-oss-20b:free" },
                new() { Name = "Gemini", Id = "google/gemma-3-27b-it:free" },
                new() { Name = "Xiaomi", Id = "xiaomi/mimo-v2-flash:free" },
                new() { Name = "Nvidia", Id = "nvidia/nemotron-nano-12b-v2-vl:free" }
            ];
}
