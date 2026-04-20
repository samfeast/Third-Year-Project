import { useStore } from "../../store/StoreProvider";

import styles from "./ConfigurePage.module.css";

export default function ConfigurePage() {
  const { state, dispatch } = useStore();

  function handleAgentDensityUpdate(agentDensity: number) {
    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: agentDensity,
        seed: state.config.seed,
        layout: state.config.layout,
      },
    });
  }

  function handleSeedUpdate(seedStr: string) {
    if (seedStr === "") return;
    const seed = parseInt(seedStr);
    if (isNaN(seed)) {
      alert("Seed must be an integer");
    } else {
      dispatch({
        type: "SET_CONFIG",
        payload: {
          agentDensity: state.config.agentDensity,
          seed: seed,
          layout: state.config.layout,
        },
      });
    }
  }

  return (
    <div>
      <h2>Current agent density {state.config.agentDensity}</h2>
      <button
        className={styles["button"]}
        onClick={() => handleAgentDensityUpdate(0.01)}
      >
        0.01 agents per m2
      </button>
      <button
        className={styles["button"]}
        onClick={() => handleAgentDensityUpdate(0.03)}
      >
        0.03 agents per m2
      </button>
      <button
        className={styles["button"]}
        onClick={() => handleAgentDensityUpdate(0.06)}
      >
        0.06 agents per m2
      </button>
      <button
        className={styles["button"]}
        onClick={() => handleAgentDensityUpdate(0.1)}
      >
        0.1 agents per m2
      </button>

      <input onChange={(e) => handleSeedUpdate(e.target.value)} />
    </div>
  );
}
