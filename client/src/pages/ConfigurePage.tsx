import { useStore } from "../store/StoreProvider";
import "./styles/ConfigurePage.css";

export default function ConfigurePage() {
  const { state, dispatch } = useStore();

  function handleStateUpdate(agentDensity: number) {
    const layout = state.config.layout;
    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: agentDensity,
        layout: layout,
      },
    });
  }
  return (
    <div>
      <h2>Current agent density {state.config.agentDensity}</h2>
      <button className="button" onClick={() => handleStateUpdate(0.01)}>
        0.01 agents per m2
      </button>
      <button className="button" onClick={() => handleStateUpdate(0.03)}>
        0.03 agents per m2
      </button>
      <button className="button" onClick={() => handleStateUpdate(0.06)}>
        0.06 agents per m2
      </button>
      <button className="button" onClick={() => handleStateUpdate(0.1)}>
        0.1 agents per m2
      </button>
    </div>
  );
}
