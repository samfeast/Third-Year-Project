using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{
    private readonly bool _useCdt = true;
    
    public NavMesh GenerateNavMesh(Polygon positive, List<Polygon> negatives)
    {
        // Toggle between CDT (from Clipper2) or ear clipping (own implementation)
        ITriangulator triangulator;
        if (_useCdt)
            triangulator = new ConstrainedDelaunayTriangulator();
        else
            triangulator = new EarClippingTriangulator();
        
        List<Triangle> triangles = triangulator.Triangulate(positive, negatives);

        NavMesh navMesh = ConstructNavMeshObject(triangles);
        
        return navMesh;
    }

    private static NavMesh ConstructNavMeshObject(List<Triangle> triangles)
    {
        NavMesh navMesh = new NavMesh();

        // Create a node from every triangle in the triangulation
        for (int i = 0; i < triangles.Count; i++)
        {
            var node = new NavMesh.Node(triangles[i]);
            var index = navMesh.AddNode(node);
            Debug.Assert(index == i, "Navmesh nodes must be ordered the same as triangles");
        }
        
        Dictionary<EdgeKey, SharedEdge> edgeMap = BuildEdgeMap(triangles);
        foreach (var (_, sharedEdge) in edgeMap)
        {
            // If there is no second triangle the edge is only used by one triangle, so no neighbours need to be added
            if (sharedEdge.TriangleIndex2 == -1)
                continue;
            
            navMesh.AddNeighbour(sharedEdge);
        }

        return navMesh;
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