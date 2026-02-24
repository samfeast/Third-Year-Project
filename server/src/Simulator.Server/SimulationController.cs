using Microsoft.AspNetCore.Mvc;
using Simulator.Core;
using Simulator.Server.ManagerCommands;

namespace Simulator.Server;

[ApiController]
[Route("sim")]
public class SimulationController(SimulationManager manager) : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create()
    {
        var id = Guid.NewGuid();

        manager.EnqueueCommand(
            new CreateSimulationCommand(id, SimulationConfig.Preset1, 0)
        );

        return Ok(id);
    }

    [HttpPost("{id:guid}/stop")]
    public IActionResult Stop(Guid id)
    {
        manager.EnqueueCommand(new StopSimulationCommand(id));
        return Ok();
    }
}