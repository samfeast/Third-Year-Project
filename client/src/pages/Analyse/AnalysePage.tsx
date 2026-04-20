import { Application, extend } from "@pixi/react";
import { Container, Graphics } from "pixi.js";

import { useStore } from "../../store/StoreProvider";
import { getHeatMap } from "../../websocket/simulationCommands";
import { useHeatMap } from "../../features/analysis/useHeatMap";
import DrawLayout from "../Simulate/DrawLayout";
import { GetScaleAndOffset } from "../Simulate/SimulationCanvas";
import DrawHeatMap from "./DrawHeatMap";

export default function AnalysePage() {
  const { state } = useStore();
  const heatMap = useHeatMap();

  const canvasWidth = 1400;
  const canvasHeight = 700;

  const layout = state.config.layout;

  const { scale, offsetX, offsetY } = GetScaleAndOffset(
    layout,
    canvasWidth,
    canvasHeight,
  );

  return (
    <div>
      <button onClick={() => getHeatMap(state.clientId)}>Get Heat Map</button>

      <Application
        width={canvasWidth}
        height={canvasHeight}
        background={0xaaaaaa}
      >
        <container scale={{ x: scale, y: -scale }} x={offsetX} y={offsetY}>
          <DrawLayout layout={layout} />

          {heatMap && <DrawHeatMap heatMap={heatMap} />}
        </container>
      </Application>
    </div>
  );
}
