using System.Collections.Concurrent;
using Simulator.Core;
using Simulator.Server.ManagerCommands;
using Simulator.Server.SimulationCommands;

namespace Simulator.Server;

public class SimulationManager
{
    private readonly Dictionary<Guid, Simulator> _simulators = new();
    private readonly Dictionary<Guid, ConcurrentQueue<ISimulationCommand>> _simulationCommandQueues = new();
    private readonly ConcurrentQueue<IManagerCommand> _simulationManagerCommands = new();
    
    public Simulator GetSimulator(Guid id)
    {
        return _simulators[id];
    }
    
    public async Task RunLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ProcessManagerCommands();
            
            // Step all running simulations
            foreach (var (id, simulator) in _simulators)
            {
                StepSimulator(id, simulator);
            }
            
            await Task.Yield();
        }
    }

    private void StepSimulator(Guid id, Simulator simulator)
    {
        var engine = simulator.Engine;
        ProcessSimulationCommands(id);

        if (simulator.Status != SimulationStatus.Running) return;
                
        var snapshot = engine.StepSimulation();
        
        if (snapshot.Step % 100 == 0)
            Console.WriteLine($"Simulation {id} on step {snapshot.Step}");
        
        if (snapshot.AllComplete)
        {
            Console.WriteLine($"Simulation {id} finished on step {snapshot.Step}");
            simulator.Status = SimulationStatus.Finished;
        }
                
        simulator.AddSnapshot(snapshot);
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