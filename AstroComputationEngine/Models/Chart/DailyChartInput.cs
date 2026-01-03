using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Models.Chart;

public record DailyChartInput(TimeLocation Birth, TimeLocation Current);
