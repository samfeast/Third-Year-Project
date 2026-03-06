import { useStore } from "../store/StoreProvider";
import { emptyLayout } from "../store/types";
import { create } from "../websocket/simulationCommands";

export default function TestPage() {
  const { state, dispatch } = useStore();

  function handleStateUpdate(agentDensity: number) {
    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: agentDensity,
        layout: emptyLayout,
      },
    });
  }

  return (
    <div>
      <h1>Test</h1>

      <button onClick={() => create(state.config)}>Send layout over WS</button>
      <button onClick={() => handleStateUpdate(0.1)}>
        Set agent density 0.1
      </button>
      <button onClick={() => handleStateUpdate(0.3)}>
        Set agent density 0.3
      </button>
      <button onClick={() => handleStateUpdate(0.6)}>
        Set agent density 0.6
      </button>
      <p>Current agent density {state.config.agentDensity}</p>
      <p>Websocket connection: {state.connectionStatus}</p>
    </div>
  );
}
