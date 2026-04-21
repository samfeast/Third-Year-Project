using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Utils;

public class ResultsCollector
{
    public readonly Heatmap Heatmap = new();
    public readonly List<(byte[][] snapshot, int step)> HeatmapSnapshots = [];
    public List<int> Evacuationtimes = [];

    public struct BlurredHeatmap
    {
        public required float[][] Heatmap;
        public Vector2Int Origin;
        public int CellSize;
        public int Width;
        public int Height;
    }

    public void TakeHeatmapSnapshot(int step)
    {
        var grid = Heatmap.Grid;
        var width = grid.Length;
        var snapshot = new byte[width][];

        for (int x = 0; x < width; x++)
        {
            snapshot[x] = new byte[grid[x].Length];
            // Clamp the values to 0-255 - shouldn't be reached unless snapshot interval is very large
            for (int y = 0; y < grid[x].Length; y++)
                snapshot[x][y] = Math.Min(grid[x][y], (byte)255);
        }

        HeatmapSnapshots.Add((snapshot, step));

        Heatmap.Clear();
    }

    public int[][] HeatmapSum(IEnumerable<byte[][]> arrs)
    {
        var first = arrs.First();
    
        var width = first.Length;
        var height = first[0].Length;

        var result = new int[width][];
        for (int x = 0; x < width; x++)
            result[x] = new int[height];

        foreach (var arr in arrs)
        {
            if (arr.Length != width || arr[0].Length != height) throw new ArgumentException("Arrays must have the same dimensions");
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x][y] += arr[x][y];
                }
            }
        }

        return result;
    }

    public float[][] GaussianBlur(int[][] arr, bool[][] mask)
    {
        var height = arr.Length;
        var width = arr[0].Length;

        var result = new float[height][];
        for (int i = 0; i < height; i++)
            result[i] = new float[width];

        // Kernel radius of 2 giving a 5x5 kernal
        const int radius = 2;
        const float sigma = 2.5f;

        var kernel = CreateKernel(radius, sigma);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Ignore masked cells
                if (!mask[y][x])
                    continue;

                var sum = 0f;
                var weightSum = 0f;

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int ny = y + ky;
                        int nx = x + kx;

                        // Bounds check
                        if (ny < 0 || ny >= height || nx < 0 || nx >= width)
                            continue;

                        // Optional: only include masked pixels in blur
                        if (!mask[ny][nx])
                            continue;

                        var weight = kernel[ky + radius, kx + radius];
                        sum += arr[ny][nx] * weight;
                        weightSum += weight;
                    }
                }

                // Normalize (important when skipping masked pixels)
                result[y][x] = weightSum > 0 ? sum / weightSum : arr[y][x];
            }
        }

        return Normalize(result);
    }
    
    private float[,] CreateKernel(int radius, float sigma)
    {
        var size = radius * 2 + 1;
        var kernel = new float[size, size];

        var sigma2 = 2 * sigma * sigma;
        var sum = 0f;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                var value = (float)Math.Exp(-(x * x + y * y) / sigma2);
                kernel[y + radius, x + radius] = value;
                sum += value;
            }
        }

        // Normalize kernel
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            kernel[y, x] /= sum;

        return kernel;
    }

    private float[][] Normalize(float[][] arr)
    {
        var max = arr.Max(row => row.Max());
        return arr.Select(row => row.Select(value => value / max).ToArray()).ToArray();
    }

}