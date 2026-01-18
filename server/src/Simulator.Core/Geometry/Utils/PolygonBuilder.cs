using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

public static class PolygonBuilder
{
    private struct HoleInfo
    {
        public Polygon hole;
        public int holeIndex;
        public int maxX;
        public int maxXIndex;
    }
    
    // Take list of input polygons and return correctly ordered vertex list
    public static LinkedList<Vector2Int> BuildVertexList(Polygon positive, List<Polygon> negatives)
    {
        var vertices = new LinkedList<Vector2Int>();

        List<HoleInfo> holes = BuildHoleInfo(negatives);
        
        // Sort negative polygons with the rightmost polygon first
        // If two polygons have the same rightmost X coordinate use index as a tiebreaker to guarantee determinism
        holes.Sort((a, b) =>
        {
            int cmp = b.maxX.CompareTo(a.maxX);
            if (cmp != 0) return cmp;
            return a.holeIndex.CompareTo(b.holeIndex);
        });
        
        foreach (var vertex in positive.vertices)
        {
            vertices.AddLast(vertex);
        }

        foreach (var hole in holes)
        {
            var visibleVertex = FindMutuallyVisibleVertex(vertices, hole);

            vertices = SpliceVertices(vertices, visibleVertex, hole);
        }

        return vertices;
    }
    
    private static List<HoleInfo> BuildHoleInfo(List<Polygon> holes)
    {
        List<HoleInfo> holeInfo = new();
        // Go through each hole and calculate the index and value of the vertex with greatest X value
        for (int i = 0; i < holes.Count; i++)
        {
            var (maxX, maxIndex) = GetMaxX(holes[i].vertices);

            holeInfo.Add(new HoleInfo
            {
                hole = holes[i],
                holeIndex = i,
                maxX = maxX,
                maxXIndex = maxIndex,
            });
        }

        return holeInfo;
    }
    
    // Get the greatest X coordinate from a list of vertices and its index
    private static (int, int) GetMaxX(List<Vector2Int> vertices)
    {
        int maxX = vertices[0].X;
        int maxIndex = 0;
        
        for (int i = 1; i < vertices.Count; i++)
        {
            int x = vertices[i].X;
            if (x > maxX)
            {
                maxX = x;
                maxIndex = i;
            }
        }
        
        return (maxX, maxIndex);
    }
    
    private static LinkedListNode<Vector2Int> FindMutuallyVisibleVertex(LinkedList<Vector2Int> outerVertices, HoleInfo holeInfo)
    {
        Debug.Assert(outerVertices.Count > 0, "No outer vertices to check for visibility");
        // Get the vertex of the polygon with the largest x coordinate (the origin of the raycast)
        Vector2Int M = holeInfo.hole.vertices[holeInfo.maxXIndex];
        Console.WriteLine($"M = {M}");
        
        var (nearestIntersectionX, nearestT, edgeStart) = FindNearestIntersection(outerVertices, M);
        var edgeEnd = (edgeStart.Next ?? outerVertices.First)!;

        // If T = 0 or T = 1 the ray hit a vertex exactly
        if (nearestT.IsZero || nearestT.IsOne)
        {
            // FindNearestIntersection() ignores horizontal edges, so we know the vertices at each end of the edge have
            // different y coordinates - this makes it safe to check this way
            if (M.Y == edgeStart.Value.Y) return edgeStart;
            if (M.Y == edgeEnd.Value.Y) return edgeEnd;
        }
        
        // The 'test region' is the triangle formed by the origin of the raycast (M), the intersection point (I), and
        // the rightmost vertex on the intersecting edge (P)
        var I = new Vector2Fraction(nearestIntersectionX, new LongFraction(M.Y, 1));
        var P = edgeStart.Value.X > edgeEnd.Value.X ? edgeStart : edgeEnd;

        var testRegion = new TriangleFraction(M.ToVector2Fraction(), I, P.Value.ToVector2Fraction());

        // Find any vertices in the test region. If there aren't any, P is mutually visible
        var obstructingVertices = GetObstructingVertices(outerVertices, testRegion);
        if (obstructingVertices.Count == 0) return P;
        
        // Otherwise return the obstructing vertex with the minimum angle to the vector MI
        return FindMinimumAngleVertex(obstructingVertices, M);
    }
    
    // Finds where a ray cast in the +ve x direction from M intersects with the polygon defined by outervertices
    // Returns the x coordinate of the intersection and t value from parametric equation, both as LongFraction's
    // Also returns the node where the edge starts (the edge ends at the next node in the linked list)
    private static (LongFraction, LongFraction, LinkedListNode<Vector2Int>) FindNearestIntersection(
        LinkedList<Vector2Int> outerVertices, Vector2Int M)
    {
        var nearestIntersectionX = new LongFraction(1, 0);
        var nearestT = new LongFraction(1, 0);
        LinkedListNode<Vector2Int> edgeStart = outerVertices.First!;
        
        var node = outerVertices.First;
        while (node != null)
        {
            var A = node.Value;
            var B = (node.Next ?? outerVertices.First)!.Value;

            if (!IsValidEdge(M, A, B))
            {
                node = node.Next;
                continue;
            }

            // This is a convoluted way of checking if the x coordinate of the intersection between ray and edge is on
            // the left of the ray origin. This approach avoids division so everything is kept as integers
            // I.e: discard the edge if Ix < M.X, where Ix = A.X + t * (B.X - A.X) and t = (M.Y - A.Y) / (B.Y - A.Y)
            int dy = B.Y - A.Y;
            long lhs = (M.Y - A.Y) * (B.X - A.X);
            long rhs = (M.X - A.X) * dy;

            if (dy > 0 && lhs <= rhs || dy < 0 && lhs >= rhs)
            {
                node = node.Next;
                continue;
            }

            // Safe approach to checking if the candidate intersection is closer to the ray origin than the current
            // closest intersection. Avoids floating point values.
            long candidateIxNumerator = A.X * (B.Y - A.Y) + (M.Y - A.Y) * (B.X - A.X);
            long candidateIxDenominator = B.Y - A.Y;
            var candidateIntersectionX = new LongFraction(candidateIxNumerator, candidateIxDenominator);

            if (candidateIntersectionX < nearestIntersectionX)
            {
                nearestIntersectionX = candidateIntersectionX;
                edgeStart = node;
                nearestT = new LongFraction(M.Y - A.Y, B.Y - A.Y);
                Console.WriteLine($"{A}-{B} intersects ray at X={nearestIntersectionX.Compute()} (new nearest)");
            }
            
            node = node.Next;
        }
        
        return (nearestIntersectionX, nearestT, edgeStart);
    }
    
    private static bool IsValidEdge(Vector2Int M, Vector2Int A, Vector2Int B)
    {
        if (A.Y == B.Y) return false; // Horizontal edges are not valid
        if (Math.Min(A.Y, B.Y) > M.Y || 
            Math.Max(A.Y, B.Y) <= M.Y) return false; // Edges entirely above or below the ray are not valid
        if (Cross(B - A, M - A) <= 0) return false; // Edges exterior to ray are not valid

        return true;
    }
    
    // 2D cross product to determine convexity or whether a v2 lies to the left or right of directed edge v0->v1
    private static int Cross(Vector2Int a, Vector2Int b)
    {
        return a.X * b.Y - a.Y * b.X;
    }
    
    private static List<LinkedListNode<Vector2Int>> GetObstructingVertices(LinkedList<Vector2Int> outerVertices, TriangleFraction testRegion)
    {
        var obstructingVertices = new List<LinkedListNode<Vector2Int>>();
        
        var node = outerVertices.First;
        while (node != null)
        {
            var prevNode = node.Previous ?? outerVertices.Last!;
            var nextNode = node.Next ?? outerVertices.First!;
            // Only need to check if reflex vertices are obstructing
            if (IsConvex(prevNode.Value, node.Value, nextNode.Value))
            {
                node = node.Next;
                continue;
            }

            var testPoint = node.Value.ToVector2Fraction();
            if (testRegion.ContainsPoint(testPoint))
                obstructingVertices.Add(node);
            
            node = node.Next;
        }

        return obstructingVertices;
    }
    
    private static bool IsConvex(Vector2Int prev, Vector2Int curr, Vector2Int next)
    {
        return Cross(curr - prev, next - curr) > 0;
    }

    private static LinkedListNode<Vector2Int> FindMinimumAngleVertex(List<LinkedListNode<Vector2Int>> vertices, Vector2Int M)
    {
        var minScore = new LongFraction(0, 1);
        var minAngleVertex = vertices[0];
        foreach (var vertex in vertices)
        {
            var numerator = (long)(vertex.Value.X - M.X) * (vertex.Value.X - M.X);
            var denominator = numerator + vertex.Value.Y * vertex.Value.Y;

            var score = new LongFraction(numerator, denominator);
            if (score < minScore)
            {
                minScore = score;
                minAngleVertex = vertex;
            }
        }
        
        return minAngleVertex;
    }
    
    private static LinkedList<Vector2Int> SpliceVertices(LinkedList<Vector2Int> vertices,
        LinkedListNode<Vector2Int> visibleVertex, HoleInfo holeInfo)
    {
        List<Vector2Int> holeVertices = holeInfo.hole.vertices;
        int numHoleVertices = holeVertices.Count;
        LinkedListNode<Vector2Int> lastVertex = visibleVertex;
        // <= because we want to include M twice to close the loop
        for (int i = 0; i <= numHoleVertices; i++)
        {
            Vector2Int vertex = holeVertices[(holeInfo.maxXIndex + i) % numHoleVertices];
            lastVertex = vertices.AddAfter(lastVertex, vertex);
        }
        
        vertices.AddAfter(lastVertex, visibleVertex.Value);

        return vertices;
    }
}