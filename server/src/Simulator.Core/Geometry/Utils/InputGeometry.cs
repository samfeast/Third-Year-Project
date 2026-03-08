using Simulator.Core.Geometry.Shapes;

namespace Simulator.Core.Geometry.Utils;

public record InputGeometry
{
    public readonly Polygon Positive;
    public readonly List<Polygon> Negatives;
    public readonly double Area;
    
    public InputGeometry(Polygon positive, List<Polygon> negatives)
    {
        Positive = positive;
        Negatives = negatives;
        
        var area = positive.GetArea();
        foreach (var negative in negatives)
        {
            area -=  negative.GetArea();
        }

        Area = area;
    }
    // Constructor for empty geometry
    public InputGeometry() 
        : this(new Polygon(), []) // Calls primary constructor
    {
    }
}