namespace Simulator.Core.Geometry;

public class UniformGrid(int cellSize)
{
    public readonly int CellSize = cellSize;
    private readonly double _reciprocalCellSize = 1.0f / cellSize;
    
    private readonly Dictionary<(int cellX, int cellY), List<int>> _grid = new();

    public void Add(int cellX, int cellY, int value)
    {
        if (_grid.TryGetValue((cellX, cellY), out var list))
            list.Add(value);
        else
            _grid[(cellX, cellY)] = [value];
    }

    public List<int> Get(double x, double y)
    {
        var cellX = (int)(x * _reciprocalCellSize);
        var cellY = (int)(y * _reciprocalCellSize);

        if (_grid.TryGetValue((cellX, cellY), out var cellValue))
            return cellValue;

        return [];
    }
    
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