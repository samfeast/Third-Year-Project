import SimulationCanvas from "../components/SimulationCanvas";
import { startPlaybackLoop } from "../playbackLoop";
import { snapshotStore } from "../store/snapshotStore";
import { useStore } from "../store/StoreProvider";
import { create, getSnapshots } from "../websocket/simulationCommands";
import "./styles/SimulatePage.css";

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
        onClick={() => snapshotStore.setPlaybackStatus("playing")}
      >
        Play
      </button>
      <button
        className="button"
        onClick={() => snapshotStore.setPlaybackStatus("paused")}
      >
        Pause
      </button>
      <SimulationCanvas />
    </div>
  );
}
