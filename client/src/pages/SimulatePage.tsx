import { useStore } from "../store/StoreProvider";
import { useSnapshotBuffer } from "../store/useSnapshotBuffer";
import { create } from "../websocket/simulationCommands";
import "./styles/SimulatePage.css";

export default function SimulatePage() {
  const { state } = useStore();
  const snapshots = useSnapshotBuffer();

  const latest = snapshots[snapshots.length - 1];
  const firstFive = latest?.positions?.slice(0, 5) ?? [];

  return (
    <div>
      <button className="button" onClick={() => create(state.config)}>
        Start Simulation
      </button>
      <p>Snapshots received: {snapshots.length}</p>

      {latest ? (
        <div>
          <h3>Latest Snapshot (step {latest.step})</h3>
          <ul>
            {firstFive.map((pos, i) => (
              <li key={i}>
                Agent {i}: x={pos[0]}, y={pos[1]}
              </li>
            ))}
          </ul>
        </div>
      ) : (
        <p>No snapshots yet</p>
      )}
    </div>
  );
}
