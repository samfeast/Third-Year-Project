using System.Collections.Concurrent;
using Simulator.Core;
using Simulator.Server.ManagerCommands;
using Simulator.Server.SimulationCommands;

namespace Simulator.Server;

public class SimulationManager
{
    public class Simulator(SimulationEngine engine, int priority)
    {
        public readonly SimulationEngine Engine = engine;
        public int Priority = priority;
        public SimulationStatus Status = SimulationStatus.Running;
    }

    public enum SimulationStatus
    {
        Running,
        Paused,
        Finished
    }

    private readonly Dictionary<Guid, Simulator> _simulators = new();
    private readonly Dictionary<Guid, ConcurrentQueue<ISimulationCommand>> _simulationCommandQueues = new();
    private readonly ConcurrentQueue<IManagerCommand> _simulationManagerCommands = new();

    public event Action<Guid, SimulationSnapshot>? SnapshotProduced;

    private readonly CancellationTokenSource _cts = new();

    public void Start()
    {
        Task.Run(RunLoop);
    }

    public void Stop()
    {
        _cts.Cancel();
    }
    
    // Enqueue manager-level command
    public void EnqueueCommand(IManagerCommand command)
    {
        _simulationManagerCommands.Enqueue(command);
    }

    // Enqueue simulation-level command
    public void EnqueueCommand(Guid id, ISimulationCommand command)
    {
        _simulationCommandQueues[id].Enqueue(command);
    }

    private async Task RunLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            ProcessManagerCommands();
            
            // Step all running simulations
            foreach (var (id, simulator) in _simulators)
            {
                var engine = simulator.Engine;
                ProcessSimulationCommands(id);

                if (simulator.Status != SimulationStatus.Running) continue;
                
                var snapshot = engine.StepSimulation();
                SnapshotProduced?.Invoke(id, snapshot);
            }

            // Wait 50ms between steps to avoid uncontrolled simulation
            await Task.Delay(50, _cts.Token);
        }
    }

    // Process manager-level commands
    private void ProcessManagerCommands()
    {
        while (_simulationManagerCommands.TryDequeue(out var command))
        {
            command.Apply(this);
        }
    }
    
    // Process simulation commands for specified simulation
    private void ProcessSimulationCommands(Guid id)
    {
        var queue = _simulationCommandQueues[id];
        var engine = _simulators[id].Engine;
        while (queue.TryDequeue(out var command))
        {
            command.Apply(engine);
        }
    }
    
    #region Command callbacks
    internal void CreateSimulationInternal(CreateSimulationCommand command)
    {
        if (_simulators.ContainsKey(command.Id))
            throw new InvalidOperationException("Simulation already exists with this GUID");

        var engine = new SimulationEngine(command.Config);
        _simulators[command.Id] = new Simulator(engine, command.Priority);
        _simulationCommandQueues[command.Id] = new ConcurrentQueue<ISimulationCommand>();
    }

    internal void StopSimulationInternal(StopSimulationCommand command)
    {
        if (_simulators.TryGetValue(command.Id, out var simulator))
        {
            simulator.Status = SimulationStatus.Finished;
        }
    }
    
    #endregion
}