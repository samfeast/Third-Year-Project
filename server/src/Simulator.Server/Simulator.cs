using Simulator.Core;

namespace Simulator.Server;

public class Simulator(SimulationEngine engine, int priority)
{
    public readonly SimulationEngine Engine = engine;
    public int Priority = priority;
    public SimulationStatus Status = SimulationStatus.Running;

    private readonly List<SimulationSnapshot> _snapshots = new();
    private readonly object _lock = new();

    public void AddSnapshot(SimulationSnapshot snapshot)
    {
        lock (_lock)
        {
            _snapshots.Add(snapshot);
        }
    }

    public IReadOnlyList<SimulationSnapshot> GetSnapshots(int startStep, int numSteps)
    {
        lock (_lock)
        {
            if (startStep >= _snapshots.Count) return [];
            
            var endStep = Math.Min(startStep + numSteps, _snapshots.Count);
            return _snapshots.GetRange(startStep, endStep - startStep);
        }
    }
}