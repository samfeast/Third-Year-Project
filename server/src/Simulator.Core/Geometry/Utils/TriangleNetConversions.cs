using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;
using TriangleNet.Geometry;
using ExtPolygon = TriangleNet.Geometry.Polygon;
using ExtTriangle = TriangleNet.Topology.Triangle;
using Polygon = Simulator.Core.Geometry.Shapes.Polygon;
using Triangle = Simulator.Core.Geometry.Shapes.Triangle;

namespace Simulator.Core.Geometry.Utils;

public static class TriangleNetConversions
{
    public static ExtPolygon InputGeometryToExtPolygon(Polygon positive, List<Polygon> negatives)
    {
        var polygon = new ExtPolygon();
        polygon.AddPolygon(positive);
        foreach (var negative in negatives)
        {
            polygon.AddPolygon(negative);
            polygon.Holes.Add(FindInteriorPoint(negative));
            
        }

        return polygon;
    }

    public static Triangle ExtTriangleToTriangle(ExtTriangle extTriangle)
    {
        var vertices = new Vector2Int[3];
        for (int i = 0; i < 3; i++)
        {
            var vertex = extTriangle.GetVertex(i);
            vertices[i] = new Vector2Int((int)Math.Round(vertex.X), (int)Math.Round(vertex.Y));
        }
        return new Triangle(vertices[0], vertices[1], vertices[2]);
    }  

    private static void AddPolygon(this ExtPolygon target, Polygon source)
    {
        var vertices = source.Vertices;
        
        var mappedVertices = new Vertex[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            mappedVertices[i] = Vector2IntToVertex(vertices[i]);
            target.Add(mappedVertices[i]);
        }
        
        for (int i = 0; i < vertices.Count; i++)
        {
            var start = mappedVertices[i];
            var end = mappedVertices[(i + 1) % vertices.Count];

            target.Add(new Segment(start, end));
        }
    }

    // Hacky method for finding any point inside a polygon
    // Gets the centroid of the triangle formed by three consecutive vertices and checks if it is inside
    // Keeps going until it finds a working point - performance will degrade for highly concave polygons
    private static Point FindInteriorPoint(Polygon polygon)
    {
        var n = polygon.Vertices.Count;
        for (int i = 0; i < n; i++)
        {
            var triangle = new Triangle(polygon.Vertices[i], polygon.Vertices[(i + 1) % n],
                polygon.Vertices[(i + 2) % n]);

            // Ignore collinear triangles
            if (!triangle.IsValid())
                continue;

            var centroid = triangle.GetCentroid().Evaluate();
            var roundedCentroid = new Vector2Int((int)Math.Round(centroid.X), (int)Math.Round(centroid.Y));
            if (triangle.ContainsPoint(roundedCentroid))
                return new Point(roundedCentroid.X, roundedCentroid.Y);
        }

        throw new UnreachableException();
    }

    private static Vertex Vector2IntToVertex(Vector2Int v) => new(v.X, v.Y);
}