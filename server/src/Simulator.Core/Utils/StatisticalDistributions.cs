namespace Simulator.Core.Utils;

public static class StatisticalDistributions
{
    public static double SampleWeibull(Random rng, double shape, double scale)
    {
        var u = rng.NextDouble();
        return scale * Math.Pow(-Math.Log(1.0f - u), 1.0f / shape);
    }    
}