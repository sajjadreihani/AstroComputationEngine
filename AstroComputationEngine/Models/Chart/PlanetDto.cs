using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroComputationEngine.Models.Astrology;
public class PlanetDto
{
    public string Planet { get; set; }
    public double Degree { get; set; }
    public int House { get; set; }
    public string Sign { get; set; }
    public bool Retro { get; set; } = false;
}
