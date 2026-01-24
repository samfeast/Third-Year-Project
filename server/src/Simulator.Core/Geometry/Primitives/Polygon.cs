namespace Simulator.Core.Geometry.Primitives;

public class Polygon
{
    public List<Vector2Int> Vertices;

    public Polygon()
    {
        Vertices = [];
    }

    public Polygon(List<Vector2Int> v)
    {
        Vertices = v;
    }

    public List<int[]> ToListInt()
    {
        List<int[]> rtn = [];
        foreach (var vertex in Vertices)
        {
            rtn.Add([vertex.X, vertex.Y]);
        }

        return rtn;
    }

    public override string ToString()
    {
        var s = "";
        foreach (var v in Vertices)
            s +=  v + ", ";

        return s;
    }
    
    
}