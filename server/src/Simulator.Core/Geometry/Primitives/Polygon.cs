namespace Simulator.Core.Geometry.Primitives;

public class Polygon
{
    public List<Vector2Int> vertices;

    public Polygon()
    {
        vertices = [];
    }

    public Polygon(List<Vector2Int> v)
    {
        vertices = v;
    }

    public override string ToString()
    {
        string s = "";
        foreach (var v in vertices)
            s +=  v + ", ";

        return s;
    }
    
    
}