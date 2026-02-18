using Simulator.Core.Geometry.Shapes;
using Simulator.Core.Geometry.Triangulators;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry;

public static class NavMeshGenerator
{
    public static NavMesh GenerateNavMesh(InputGeometry inputGeometry, int gridResolution = 50)
    {
        var triangulator = new ConstrainedDelaunayTriangulator();
        List<Triangle> triangles = triangulator.Triangulate(inputGeometry);
        
        return ConstructNavMeshObject(triangles, gridResolution);
    }

    private static NavMesh ConstructNavMeshObject(List<Triangle> triangles, int gridResolution)
    {
        var navMesh = new NavMesh(gridResolution);

        // Create a node for every triangle in the triangulation and add it to the uniform grid
        for (int i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];
            var node = new NavMesh.Node(triangle);
            navMesh.Nodes.Add(node);
            navMesh.CumulativeDoubleAreas.Add(node.DoubleArea + navMesh.CumulativeDoubleAreas.LastOrDefault());
            AddToGrid(navMesh, triangle, i, gridResolution);
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
    // Could be optimised further if grid lookups become a bottleneck
    private static void AddToGrid(NavMesh navMesh, Triangle triangle, int triangleIndex, int gridResolution)
    {
        var boundingBox = triangle.GetBoundingBox();
        var x = boundingBox.MinX;
        while (x <= boundingBox.MaxX)
        {
            var y = boundingBox.MinY;
            while (y <= boundingBox.MaxY)
            {
                navMesh.Grid.Add(x, y, triangleIndex);
                y += Math.Min(gridResolution, boundingBox.MaxY - boundingBox.MinY);
            }
            x += Math.Min(gridResolution, boundingBox.MaxX - boundingBox.MinX);
        }

    }

    private static Dictionary<EdgeKey, SharedEdge> BuildEdgeMap(List<Triangle> triangles)
    {
        Dictionary<EdgeKey, SharedEdge> edgeMap = [];
        for (int i = 0; i < triangles.Count; i++)
        {
            var t = triangles[i];
            AddEdge(edgeMap, new EdgeKey(t.A, t.B), i, 0); // A->B
            AddEdge(edgeMap, new EdgeKey(t.B, t.C), i, 1); // B->C
            AddEdge(edgeMap, new EdgeKey(t.C, t.A), i, 2); // C->A
        }

        return edgeMap;
    }
    
    private static void AddEdge(Dictionary<EdgeKey, SharedEdge> edgeMap, EdgeKey key, int triangleIndex, int edgeIndex)
    {
        // If the shared edge already exists set the second triangle and corresponding index
        if (edgeMap.TryGetValue(key, out var edge))
        {
            edge.TriangleIndex2 = triangleIndex;
            edge.EdgeIndex2 = edgeIndex;
            edgeMap[key] = edge;
            return;
        }
        // If the edge doesn't exist create it with only the first triangle and corresponding index
        edgeMap[key] = new SharedEdge
        {
            TriangleIndex1 = triangleIndex,
            EdgeIndex1 = edgeIndex,
            TriangleIndex2 = -1,
            EdgeIndex2 = -1
        };
    }
    
    
}