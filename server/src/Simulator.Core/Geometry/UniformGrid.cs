using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class UniformGrid<T>(int cellSize)
{
    public readonly int CellSize = cellSize;
    private readonly double _reciprocalCellSize = 1.0f / cellSize;
    
    private readonly Dictionary<(int cellX, int cellY), List<T>> _grid = new();
    private static readonly List<T> _empty = [];
    
    public void Add(T value, double x, double y)
    {
        var cell = ComputeCell(x, y);
        AddToCell(value, cell);
    }
    public void Add(T value, Vector2 position) => Add(value, position.X, position.Y);
    public void Add(T value, Vector2Int position) => Add(value, position.X, position.Y);
    
    public void AddToCell(T value, (int x, int y) cell)
    {
        if (_grid.TryGetValue(cell, out var list))
            list.Add(value);
        else
            _grid[cell] = [value];
    }

    public List<T> Get(double x, double y)
    {
        var cell = ComputeCell(x, y);
        return _grid.GetValueOrDefault(cell, _empty);
    }

    public void RemoveFromCell(T value, (int x, int y) cell)
    {
        if (_grid.TryGetValue(cell, out var list))
        {
            int index = list.IndexOf(value);
            if (index < 0) return; // Value not found
            // Avoids shifting list with Remove() (copy last element into deleted index, then remove last)
            int last = list.Count - 1;
            list[index] = list[last];
            list.RemoveAt(last);
                
            if (list.Count == 0)
                _grid.Remove(cell);
        }
    }

    public (int X, int Y) ComputeCell(double x, double y)
    {
        var cellX = (int)Math.Floor(x * _reciprocalCellSize);
        var cellY = (int)Math.Floor(y * _reciprocalCellSize);
        return (cellX, cellY);
    }
    public (int X, int Y) ComputeCell(Vector2 position) => ComputeCell(position.X, position.Y);
    public (int X, int Y) ComputeCell(Vector2Int position) => ComputeCell(position.X, position.Y);
    
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("{");

        foreach (var (key, values) in _grid.OrderBy(k => k.Key.cellX).ThenBy(k => k.Key.cellY))
        {
            sb.Append("  (")
                .Append(key.cellX)
                .Append(", ")
                .Append(key.cellY)
                .Append(") : [");

            if (values.Count > 0)
                sb.Append(string.Join(", ", values));

            sb.AppendLine("]");
        }

        sb.Append("}");
        return sb.ToString();
    }
}