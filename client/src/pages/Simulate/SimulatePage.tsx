import { useStore } from "../../store/StoreProvider";

import { snapshotStore } from "../../features/simulation/snapshotStore";
import { startSimulation } from "../../features/simulation/startSimulation";

import SimulationCanvas from "./SimulationCanvas";

import "./SimulatePage.css";

export default function SimulatePage() {
  const { state } = useStore();

  return (
    <div>
      <button
        className="button"
        onClick={() => {
          startSimulation(state.clientId, state.config);
        }}
      >
        Start Simulation
      </button>
      <button
        className="button"
        onClick={() => snapshotStore.setPlaybackState("playing")}
      >
        Play
      </button>
      <button
        className="button"
        onClick={() => snapshotStore.setPlaybackState("paused")}
      >
        Pause
      </button>
      <SimulationCanvas />
    </div>
  );
}
