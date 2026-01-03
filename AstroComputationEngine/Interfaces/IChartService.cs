using AstroComputationEngine.Models.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroComputationEngine.Interfaces;
public interface IChartService
{
    string GenerateDaily(DailyChartInput input);
    string GenerateMoment(DailyChartInput input);
    string GenerateComposite(RelationalChartInput input);
    string GenerateDavison(RelationalChartInput input);
}
