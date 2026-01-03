using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroComputationEngine.Models.Astrology;
public class AspectDto
{
    public string PlanetA { get; set; }
    public string PlanetB { get; set; }
    public double Orb { get; set; }
    public string Type { get; set; } // e.g., Conjunction, Square, etc.
}