namespace Simulator.Core.Geometry.Primitives;

public class Polygon
{
    public List<Vector2Int> vertices;
    public Direction direction;

    public Polygon()
    {
        vertices = [];
        direction = Direction.CCW;
    }
    
    
}