import './App.css'
import SimulationCanvas from './components/SimulationCanvas'
import { useSimulationWebSocket } from './hooks/useWebSocket'
import type { Agent } from './components/DrawAgents'
import { useState } from 'react'

import { preset1Floorplan } from './presets/preset1'
import { preset2Floorplan } from './presets/preset2'
import { preset3Floorplan } from './presets/preset3'
import { preset4Floorplan } from './presets/preset4'

export type Snapshot = {
  agents: Agent[]
  step: number
}

function App() {

  const { snapshot, sendCommand } = useSimulationWebSocket("ws://localhost:5158/ws");

  const [floorplan, setFloorplan] = useState(preset1Floorplan)

  const handleStart = (presetNumber: number) => {
    // send command to server
    sendCommand("create", presetNumber);

    // update floorplan based on preset number
    switch (presetNumber) {
      case 1:
        setFloorplan(preset1Floorplan);
        break;
      case 2:
        setFloorplan(preset2Floorplan);
        break;
      case 3:
        setFloorplan(preset3Floorplan);
        break;
      case 4:
        setFloorplan(preset4Floorplan);
        break;
    }
  };
  
  return (
    <div>
      <h3>Evacuation Simulator</h3>
      <p>Step: {snapshot.step} ({snapshot.step/10}s)</p>

      <div style={{ marginBottom: "10px" }}>
        <button onClick={() => handleStart(1)}>Start 1</button>
        <button onClick={() => handleStart(2)}>Start 2</button>
        <button onClick={() => handleStart(3)}>Start 3</button>
        <button onClick={() => handleStart(4)}>Start 4</button>
      </div>

      <SimulationCanvas
        floorplan={floorplan}
        agents={snapshot.agents}
      />
    </div>
  )
}

export default App
