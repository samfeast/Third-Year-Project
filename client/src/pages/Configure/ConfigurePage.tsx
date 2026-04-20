import { useMemo, useState } from "react";
import { useStore } from "../../store/StoreProvider";

import styles from "./ConfigurePage.module.css";
import Slider from "../../components/Slider/Slider";
import ShuffleIcon from "../../components/ShuffleIcon/ShuffleIcon";
import ToggleSwitch from "../../components/ToggleSwitch/ToggleSwitch";
import { approximateGamma } from "../../utils/GammaApproximation";

export default function ConfigurePage() {
  const { state, dispatch } = useStore();
  const [useCsv, setUseCsv] = useState(false);

  const [agentRadiusPosition, setAgentRadiusPosition] = useState(
    agentRadiusToPosition(state.config.agentRadius),
  );
  const [agentDensityPosition, setAgentDensityPosition] = useState(
    agentDensityToPosition(state.config.agentDensity ?? 0.1),
  );
  const [agentSpeedShapePosition, setAgentSpeedShapePosition] = useState(
    agentSpeedShapeToPosition(state.config.speedShape),
  );
  const [agentSpeedScalePosition, setAgentSpeedScalePosition] = useState(
    agentSpeedScaleToPosition(state.config.speedScale),
  );
  const [exitRadiusPosition, setExitRadiusPosition] = useState(
    exitRadiusToPosition(state.config.exitRadius),
  );

  const [customPositionsChecked, setCustomPositionsChecked] = useState(
    state.config.agentStartPositions.length > 0,
  );

  const stats = useMemo(() => {
    const shape = state.config.speedShape;
    const scale = state.config.speedScale;
    // Simple approximation for the mean/percentiles
    const mean = scale * approximateGamma(1 + 1 / shape);

    const p05 = scale * Math.pow(-Math.log(0.95), 1 / shape);
    const p25 = scale * Math.pow(-Math.log(0.75), 1 / shape);
    const p75 = scale * Math.pow(-Math.log(0.25), 1 / shape);
    const p95 = scale * Math.pow(-Math.log(0.05), 1 / shape);
    return {
      mean: mean.toFixed(2),
      p05: p05.toFixed(2),
      p25: p25.toFixed(2),
      p75: p75.toFixed(2),
      p95: p95.toFixed(2),
    };
  }, [state.config.speedShape, state.config.speedScale]);

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
          agentStartPositions: state.config.agentStartPositions,
          agentRadius: state.config.agentRadius,
          seed: seed,
          speedShape: state.config.speedShape,
          speedScale: state.config.speedScale,
          exitRadius: state.config.exitRadius,
          layout: state.config.layout,
        },
      });
    }
  }

  function handleAgentRadiusChange(newPosition: number) {
    if (agentRadiusPosition === newPosition) return;
    setAgentRadiusPosition(newPosition);
    const agentRadius = positionToAgentRadius(newPosition);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: state.config.speedScale,
        exitRadius: state.config.exitRadius,
        layout: state.config.layout,
      },
    });
  }

  function positionToAgentRadius(position: number) {
    return Math.round((950 * position + 50) / 5) * 5;
  }

  function agentRadiusToPosition(radius: number) {
    return (radius - 50) / 950;
  }

  function formatAgentRadius(position: number) {
    return positionToAgentRadius(position) + "mm";
  }

  function handleAgentDensityChange(newPosition: number) {
    if (agentDensityPosition === newPosition) return;
    setAgentDensityPosition(newPosition);
    const agentDensity = positionToAgentDensity(newPosition);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: agentDensity,
        agentStartPositions: [],
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: state.config.speedScale,
        exitRadius: state.config.exitRadius,
        layout: state.config.layout,
      },
    });
  }

  function positionToAgentDensity(position: number) {
    return Math.round((0.19 * position + 0.01) * 1000) / 1000;
  }

  function agentDensityToPosition(density: number) {
    return (density - 0.01) / 0.19;
  }

  function formatAgentDensity(position: number) {
    return positionToAgentDensity(position) + " per m²";
  }

  function handleAgentSpeedShapeChange(newPosition: number) {
    if (agentSpeedShapePosition === newPosition) return;
    setAgentSpeedShapePosition(newPosition);
    const agentSpeedShape = positionToAgentSpeedShape(newPosition);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: agentSpeedShape,
        speedScale: state.config.speedScale,
        exitRadius: state.config.exitRadius,
        layout: state.config.layout,
      },
    });
  }

  function positionToAgentSpeedShape(position: number) {
    return Math.round((10 * position + 6) * 50) / 50;
  }

  function agentSpeedShapeToPosition(speedShape: number) {
    return (speedShape - 6) / 10;
  }

  function formatAgentSpeedShape(position: number) {
    return positionToAgentSpeedShape(position).toString();
  }

  function handleAgentSpeedScaleChange(newPosition: number) {
    if (agentSpeedScalePosition === newPosition) return;
    setAgentSpeedScalePosition(newPosition);
    const agentSpeedScale = positionToAgentSpeedScale(newPosition);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: agentSpeedScale,
        exitRadius: state.config.exitRadius,
        layout: state.config.layout,
      },
    });
  }

  function positionToAgentSpeedScale(position: number) {
    return Math.round((position + 1) * 100) / 100;
  }

  function agentSpeedScaleToPosition(speedScale: number) {
    return speedScale - 1;
  }

  function formatAgentSpeedScale(position: number) {
    return positionToAgentSpeedScale(position).toString();
  }

  function handleExitRadiusChange(newPosition: number) {
    if (exitRadiusPosition === newPosition) return;
    setExitRadiusPosition(newPosition);
    const exitRadius = positionToExitRadius(newPosition);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: state.config.speedScale,
        exitRadius: exitRadius,
        layout: state.config.layout,
      },
    });
  }

  function positionToExitRadius(position: number) {
    return Math.round((1000 * position + 200) / 5) * 5;
  }

  function exitRadiusToPosition(exitRadius: number) {
    return (exitRadius - 200) / 1000;
  }

  function formatExitRadius(position: number) {
    return positionToExitRadius(position) + "mm";
  }

  const handleCsvUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;

      // Parse CSV: Filter out empty lines, split by comma, convert to numbers
      const positions = text
        .split("\n")
        .map((line) => line.trim())
        .filter((line) => line.length > 0)
        .map((line) => {
          const [x, y] = line.split(",").map(Number);
          return [x, y] as [number, number];
        })
        .filter(([x, y]) => !isNaN(x) && !isNaN(y));

      dispatch({
        type: "SET_CONFIG",
        payload: {
          agentDensity: 0.05,
          agentStartPositions: positions,
          agentRadius: state.config.agentRadius,
          seed: state.config.seed,
          speedShape: state.config.speedShape,
          speedScale: state.config.speedScale,
          exitRadius: state.config.exitRadius,
          layout: state.config.layout,
        },
      });
    };
    reader.readAsText(file);
  };

  return (
    <div className={styles["config-page-wrapper"]}>
      <div className={styles["main-layout"]}>
        {/* Inputs */}
        <div className={styles["controls-column"]}>
          <section className={styles["config-card"]}>
            <h2>Simulation Parameters</h2>

            <div className={styles["input-group"]}>
              <label>Random Seed</label>
              <div className={styles["seed-inline-group"]}>
                <input
                  type="number"
                  className={styles["seed-input"]}
                  value={state.config.seed}
                  style={{ fontSize: 18 }}
                  onChange={(e) => handleSeedUpdate(e.target.value)}
                />
                <button
                  className={styles["shuffle-btn"]}
                  onClick={() => {
                    const randomSeed = Math.floor(Math.random() * 1000000);
                    handleSeedUpdate(randomSeed.toString());
                  }}
                >
                  <ShuffleIcon size={20} strokeWidth={20} />
                </button>
              </div>
            </div>

            <div className={styles["input-group"]}>
              <label>Agent Radius: {state.config.agentRadius}mm</label>
              <div className={styles["slider-container"]}>
                <Slider
                  position={agentRadiusPosition}
                  width={12}
                  length={450}
                  orientation={"horizontal"}
                  onChangeCommitted={handleAgentRadiusChange}
                  formatPosition={formatAgentRadius}
                  styles={{
                    trackBorderRadius: 12,
                    trackColour: "#464C5A",
                    tooltipOffset: 8,
                    tooltipFontSize: 14,
                    fillColour: "#5865f2",
                    handleWidth: 18,
                    handleLength: 18,
                    handleBorderRadius: 16,
                    handleColour: "#eee",
                  }}
                />
              </div>
            </div>

            <div className={styles["input-group"]}>
              <label>
                Agent Speed (shape parameter): {state.config.speedShape}
              </label>
              <div className={styles["slider-container"]}>
                <Slider
                  position={agentSpeedShapePosition}
                  width={12}
                  length={450}
                  orientation={"horizontal"}
                  onChangeCommitted={handleAgentSpeedShapeChange}
                  formatPosition={formatAgentSpeedShape}
                  styles={{
                    trackBorderRadius: 12,
                    trackColour: "#464C5A",
                    tooltipOffset: 8,
                    tooltipFontSize: 14,
                    fillColour: "#5865f2",
                    handleWidth: 18,
                    handleLength: 18,
                    handleBorderRadius: 16,
                    handleColour: "#eee",
                  }}
                />
              </div>
            </div>

            <div className={styles["input-group"]}>
              <label>
                Agent Speed (scale parameter): {state.config.speedScale}
              </label>
              <div className={styles["slider-container"]}>
                <Slider
                  position={agentSpeedScalePosition}
                  width={12}
                  length={450}
                  orientation={"horizontal"}
                  onChangeCommitted={handleAgentSpeedScaleChange}
                  formatPosition={formatAgentSpeedScale}
                  styles={{
                    trackBorderRadius: 12,
                    trackColour: "#464C5A",
                    tooltipOffset: 8,
                    tooltipFontSize: 14,
                    fillColour: "#5865f2",
                    handleWidth: 18,
                    handleLength: 18,
                    handleBorderRadius: 16,
                    handleColour: "#eee",
                  }}
                />
              </div>
            </div>

            <div className={styles["input-group"]}>
              <label>Exit Radius: {state.config.exitRadius}</label>
              <div className={styles["slider-container"]}>
                <Slider
                  position={exitRadiusPosition}
                  width={12}
                  length={450}
                  orientation={"horizontal"}
                  onChangeCommitted={handleExitRadiusChange}
                  formatPosition={formatExitRadius}
                  styles={{
                    trackBorderRadius: 12,
                    trackColour: "#464C5A",
                    tooltipOffset: 8,
                    tooltipFontSize: 14,
                    fillColour: "#5865f2",
                    handleWidth: 18,
                    handleLength: 18,
                    handleBorderRadius: 16,
                    handleColour: "#eee",
                  }}
                />
              </div>
            </div>

            <div className={styles["input-group"]}>
              <label>Starting Positions</label>
              <ToggleSwitch
                leftLabel={"Random"}
                rightLabel={"Custom"}
                checked={customPositionsChecked}
                onChange={setCustomPositionsChecked}
              />
            </div>
            {customPositionsChecked ? (
              <div className={styles["upload-container"]}>
                <label className={styles["file-dropzone"]}>
                  <input
                    type="file"
                    accept=".csv"
                    className={styles["hidden-input"]}
                    onChange={handleCsvUpload}
                  />
                  <div className={styles["upload-content"]}>
                    <span className={styles["upload-icon"]}>📄</span>
                    {state.config.agentStartPositions ? (
                      <div className={styles["file-info"]}>
                        <p>
                          <strong>Positions Loaded</strong>
                        </p>
                        <span>
                          {state.config.agentStartPositions.length} agents
                          detected
                        </span>
                      </div>
                    ) : (
                      <p>
                        Upload <strong>CSV</strong> Starting Positions
                      </p>
                    )}
                  </div>
                </label>
                <small className={styles["upload-hint"]}>
                  Format: X,Y per row (millimeters)
                </small>
              </div>
            ) : (
              <div className={styles["input-group"]}>
                <label>
                  Agent Density: {state.config.agentDensity} per sq. meter
                </label>
                <div className={styles["slider-container"]}>
                  <Slider
                    position={agentDensityPosition}
                    width={12}
                    length={450}
                    orientation={"horizontal"}
                    onChangeCommitted={handleAgentDensityChange}
                    formatPosition={formatAgentDensity}
                    styles={{
                      trackBorderRadius: 12,
                      trackColour: "#464C5A",
                      tooltipOffset: 8,
                      tooltipFontSize: 14,
                      fillColour: "#5865f2",
                      handleWidth: 18,
                      handleLength: 18,
                      handleBorderRadius: 16,
                      handleColour: "#eee",
                    }}
                  />
                </div>
              </div>
            )}
          </section>
        </div>

        {/* Summary */}
        <aside className={styles["summary-column"]}>
          <div className={styles["summary-card"]}>
            <h2>Config Summary</h2>
            <ul className={styles["summary-list"]}>
              <h4>Global Parameters</h4>
              <li>
                <span>Active Layout:</span>
                <strong>{state.config.layout.name}</strong>
              </li>
              <li>
                <span>Seed:</span>
                <strong>{state.config.seed}</strong>
              </li>
              <li>
                <span>No. Agents:</span>
                <strong>
                  {customPositionsChecked
                    ? state.config.agentStartPositions.length
                    : `${state.config.agentDensity} per m²`}
                </strong>
              </li>
              <li>
                <span>Exit Radius:</span>
                <strong>{state.config.exitRadius}mm</strong>
              </li>
            </ul>

            <ul className={styles["summary-list"]}>
              <h4>Agent Parameters</h4>
              <li>
                <span>Radius:</span>
                <strong>{state.config.agentRadius}mm</strong>
              </li>
              <li>
                <span>Starting Positions:</span>
                <strong>{customPositionsChecked ? "Custom" : "Random"}</strong>
              </li>
            </ul>

            <div className={styles["speed-preview"]}>
              <h4>Preferred Speeds</h4>
              <ul className={styles["summary-list"]}>
                <li>
                  <span>5%</span>
                  <strong>≈{stats.p05}m/s</strong>
                </li>
                <li>
                  <span>25%</span>
                  <strong>≈{stats.p25}m/s</strong>
                </li>
                <li>
                  <span>50% (Mean)</span>
                  <strong>≈{stats.mean}m/s</strong>
                </li>
                <li>
                  <span>75%</span>
                  <strong>≈{stats.p75}m/s</strong>
                </li>
                <li>
                  <span>95%</span>
                  <strong>≈{stats.p95}m/s</strong>
                </li>
              </ul>
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
