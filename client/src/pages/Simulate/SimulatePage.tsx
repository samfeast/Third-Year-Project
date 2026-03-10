import { useStore } from "../../store/StoreProvider";
import { create, getSnapshots } from "../../websocket/simulationCommands";

import { startPlaybackLoop } from "../../features/simulation/playbackLoop";
import { snapshotStore } from "../../features/simulation/snapshotStore";
import SimulationCanvas from "./SimulationCanvas";

import "./SimulatePage.css";

export default function SimulatePage() {
  const { state } = useStore();

  return (
    <div>
      <button
        className="button"
        onClick={() => {
          create(state.clientId, state.config);
          startPlaybackLoop();
        }}
      >
        Start Simulation
      </button>
      <button
        className="button"
        onClick={() =>
          getSnapshots(state.clientId, {
            lastDisplayedStep: snapshotStore.getCurrentStep(),
            lastBufferedStep: snapshotStore.getLastBufferedStep(),
            playbackSpeed: 1.0,
          })
        }
      >
        Get snapshots
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
