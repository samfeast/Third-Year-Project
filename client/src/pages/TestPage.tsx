import { useStore } from "../store/StoreProvider";
import { sendStatus } from "../websocket/simulationCommands";

export default function TestPage() {
  const { state, dispatch } = useStore();

  function handleStateUpdate(inputStatus: "idle" | "running" | "finished") {
    dispatch({
      type: "SET_SIMULATION",
      payload: {
        id: "abc",
        status: inputStatus,
      },
    });
  }

  return (
    <div>
      <h1>Test</h1>

      <button onClick={() => sendStatus(state.simulation?.status)}>
        Send status over WS
      </button>
      <button onClick={() => handleStateUpdate("running")}>
        Set simulation running
      </button>
      <button onClick={() => handleStateUpdate("idle")}>
        Set simulation idle
      </button>
      <button onClick={() => handleStateUpdate("finished")}>
        Set simulation finished
      </button>
      <p>Current id: {state.simulation?.id}</p>
      <p>Current status: {state.simulation?.status}</p>
    </div>
  );
}
