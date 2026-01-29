namespace Simulator.Core.Geometry.Primitives;

public class UniformGrid(int cellHeight, int cellWidth)
{
    private readonly double _reciprocalCellHeight = 1.0 / cellHeight;
    private readonly double _reciprocalCellWidth = 1.0 / cellWidth;
    
    private readonly Dictionary<(int cellX, int cellY), List<int>> _grid = new();

    public void Add(double x, double y, int value)
    {
        var cellX = (int)Math.Floor(x * _reciprocalCellWidth);
        var cellY = (int)Math.Floor(y * _reciprocalCellHeight);

        if (_grid.TryGetValue((cellX, cellY), out var cellValue))
            cellValue.Add(value);
        else
            _grid.Add((cellX, cellY), [value]);
    }

    public List<int> Get(double x, double y)
    {
        var cellX = (int)Math.Floor(x * _reciprocalCellWidth);
        var cellY = (int)Math.Floor(y * _reciprocalCellHeight);

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