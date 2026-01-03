using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Models.Chart;

public record RelationalChartInput(TimeLocation First, TimeLocation Second, DateTime CurrentDate);
