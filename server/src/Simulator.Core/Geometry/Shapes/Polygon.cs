using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Shapes;

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
    
    public bool ContainsPoint(Vector2Int point)
    {
        var inside = false;
        for (int i = 0; i < Vertices.Count; i++)
        {
            var current = Vertices[i];
            var next = Vertices[(i + 1) % Vertices.Count];

            // Check if the point is between the Y coordinates of the edge
            var crossesY = point.Y > Math.Min(next.Y, current.Y) && point.Y <= Math.Max(next.Y, current.Y);

            if (!crossesY)
                continue;
            
            // Compute the X coordinate where the edge intersects the horizontal line at y = point.Y
            long deltaX = current.X - next.X;
            long deltaY = current.Y - next.Y;
            long intersectX = next.X + deltaX * (point.Y - next.Y) / deltaY;

            // If the point is to the left of this intersection, toggle inside/outside
            if (point.X < intersectX)
            {
                inside = !inside;
            }
        }

        return inside;
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