using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Triangulators;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry;

public static class NavMeshGenerator
{
    private const int GridSize = 50;
    public static NavMesh GenerateNavMesh(InputGeometry inputGeometry)
    {
        // Toggle between CDT (from Clipper2) or ear clipping (own implementation)
        const bool useCdt = true;
        ITriangulator triangulator;
        if (useCdt)
            triangulator = new ConstrainedDelaunayTriangulator();
        else
            triangulator = new EarClippingTriangulator();
        
        List<Triangle> triangles = triangulator.Triangulate(inputGeometry);

        NavMesh navMesh = ConstructNavMeshObject(triangles);
        
        return navMesh;
    }

    private static NavMesh ConstructNavMeshObject(List<Triangle> triangles)
    {
        var navMesh = new NavMesh(GridSize);

        // Create a node from every triangle in the triangulation
        for (int i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];
            navMesh.Nodes.Add(new NavMesh.Node(triangle));
            AddToGrid(navMesh, triangle, i);
        }
        
        Dictionary<EdgeKey, SharedEdge> edgeMap = BuildEdgeMap(triangles);
        foreach (var (_, edge) in edgeMap)
        {
            // If there is no second triangle the edge is only used by one triangle, so no neighbours need to be added
            if (edge.TriangleIndex2 == -1)
                continue;
            
            navMesh.Nodes[edge.TriangleIndex1].Neighbours[edge.EdgeIndex1] = edge.TriangleIndex2;
            navMesh.Nodes[edge.TriangleIndex2].Neighbours[edge.EdgeIndex2] = edge.TriangleIndex1;
        }

        return navMesh;
    }

    // Naive grid representation -> assign triangles to cells using their bounding box
    private static void AddToGrid(NavMesh navMesh, Triangle triangle, int triangleIndex)
    {
        var boundingBox = triangle.GetBoundingBox();
        var x = boundingBox.MinX;
        while (x < boundingBox.MaxX)
        {
            var y = boundingBox.MinY;
            while (y < boundingBox.MaxY)
            {
                navMesh.Grid.Add(x, y, triangleIndex);
                y += GridSize;
            }
            x += GridSize;
        }

    }

    private static Dictionary<EdgeKey, SharedEdge> BuildEdgeMap(List<Triangle> triangles)
    {
        Dictionary<EdgeKey, SharedEdge> edgeMap = [];
        for (int i = 0; i < triangles.Count; i++)
        {
            var t = triangles[i];
            AddEdge(edgeMap, new EdgeKey(t.A, t.B), i, 0);
            AddEdge(edgeMap, new EdgeKey(t.B, t.C), i, 1);
            AddEdge(edgeMap, new EdgeKey(t.C, t.A), i, 2);
        }

        return edgeMap;
    }
    
    private static void AddEdge(Dictionary<EdgeKey, SharedEdge> edgeMap, EdgeKey key, int triangleIndex, int edgeIndex)
    {
        if (edgeMap.TryGetValue(key, out var edge))
        {
            edge.TriangleIndex2 = triangleIndex;
            edge.EdgeIndex2 = edgeIndex;
            edgeMap[key] = edge;
            return;
        }

        edgeMap[key] = new SharedEdge
        {
            TriangleIndex1 = triangleIndex,
            EdgeIndex1 = edgeIndex,
            TriangleIndex2 = -1,
            EdgeIndex2 = -1
        };
    }
    
    
}