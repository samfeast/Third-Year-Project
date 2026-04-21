using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Utils;

public class Heatmap
{
    public byte[][] Grid = [];
    public bool[][] ValidGrid = [];
    
    public int OriginX;
    public int OriginY;
    public int CellSize;

    public int Width;
    public int Height;
    
    public void ConstructGrid(NavMesh navMesh, int cellSize)
    {
        CellSize = cellSize;
        
        // Get the bounds of the environment
        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        
        foreach (var node in navMesh.Nodes)
        {
            var triangle = node.Triangle;
            var bbox = triangle.GetBoundingBox();
            
            if (bbox.MinX < minX) minX = bbox.MinX;
            if (bbox.MaxX > maxX) maxX = bbox.MaxX;
            if (bbox.MinY < minY) minY = bbox.MinY;
            if (bbox.MaxY > maxY) maxY = bbox.MaxY;
        }
        
        OriginX = minX;
        OriginY = minY;
        
        Width = (int)Math.Ceiling((double)(maxX - minX) / cellSize);
        Height = (int)Math.Ceiling((double)(maxY - minY) / cellSize);
        
        Grid = new byte[Width][];
        ValidGrid = new bool[Width][];

        for (int x = 0; x < Width; x++)
        {
            Grid[x] = new byte[Height];
            ValidGrid[x] = new bool[Height];
        }
        
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2Int[] positionDeltas = { new(0, 0), new(0, cellSize), new(cellSize, 0), new(cellSize, cellSize) };
                Vector2Int bottomLeft = new(OriginX + x * cellSize, OriginY + y * cellSize);

                var isValid = false;
                foreach (var delta in positionDeltas)
                {
                    var point = bottomLeft + delta;
                    if (navMesh.GetCurrentNode(point).Count > 0)
                    {
                        isValid = true;
                        break;
                    }
                }

                ValidGrid[x][y] = isValid;
            }
        }
    }
    
    public void Add(Vector2Int position)
    {
        var gx = (position.X - OriginX) / CellSize;
        var gy = (position.Y - OriginY) / CellSize;
        
        // Shouldn't be possible unless there's floating point inaccuracy
        if (gx < 0 || gx >= Width || gy < 0 || gy >= Height)
            return;

        // Only count valid cells
        if (!ValidGrid[gx][gy])
            return;

        Grid[gx][gy]++;
    }
    
    public void Clear()
    { 
        foreach (var row in Grid)
            Array.Clear(row, 0, row.Length);
    }
}