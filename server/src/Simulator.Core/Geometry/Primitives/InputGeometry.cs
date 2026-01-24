namespace Simulator.Core.Geometry.Primitives;

public record InputGeometry(Polygon Positive, List<Polygon> Negatives)
{
    // Constructor for empty geometry
    public InputGeometry() 
        : this(new Polygon(), []) // Calls primary constructor
    {
    }
}