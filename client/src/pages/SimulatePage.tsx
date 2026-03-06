import SimulationCanvas from "../components/SimulationCanvas";
import { useStore } from "../store/StoreProvider";
import { useSnapshot } from "../store/useSnapshot";
import { create } from "../websocket/simulationCommands";
import "./styles/SimulatePage.css";

export default function SimulatePage() {
  const { state } = useStore();
  const latestSnapshot = useSnapshot();

  return (
    <div>
      <button className="button" onClick={() => create(state.config)}>
        Start Simulation
      </button>
      <p>Last Snapshot: Step {latestSnapshot?.step}</p>
      <SimulationCanvas
        layout={state.config.layout}
        snapshot={latestSnapshot ? latestSnapshot : undefined}
      />
    </div>
  );
}
