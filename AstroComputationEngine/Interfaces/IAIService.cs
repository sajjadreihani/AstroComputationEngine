using AstroComputationEngine.Models.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroComputationEngine.Interfaces;
public interface IAIService
{
    Task<AIResponse> AskAI(string chart, string model = "openai/gpt-oss-20b:free"); 
}
