using Simulator.Core.Geometry.Shapes;

namespace Simulator.Core.Utils;

public class ResultsCollector
{
    public struct HeatMapCell(Triangle triangle, double value)
    {
        public Triangle Triangle = triangle;
        public readonly double Value = value;
    }
    
    public List<HeatMapCell> HeatMap = [];
    public List<int> Evacuationtimes = [];
}