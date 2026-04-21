import { Application, Container, Graphics } from "pixi.js";
import { Application as App, extend } from "@pixi/react";

import { useStore } from "../../store/StoreProvider";
import { getHeatmap } from "../../websocket/simulationCommands";
import { useHeatmap } from "../../features/analysis/useHeatMap";
import DrawLayout from "../Simulate/DrawLayout";
import { GetScaleAndOffset } from "../Simulate/SimulationCanvas";
import DrawHeatMap from "./DrawHeatMap";

import styles from "./AnalysePage.module.css";
import { useEffect, useRef } from "react";
import { buildHeatmapMesh } from "../../features/analysis/buildHeatmapMesh";
import { HeatmapLayer } from "./HeatmapLayer";
import type { Layout } from "../../features/layout/types";
import type { Point } from "../../types/types";
import { emptyLayout } from "../../features/layout/defaults";

extend({ Graphics, Container });

export default function AnalysePage() {
  const { state } = useStore();
  const heatmap = useHeatmap();

  const layout = state.config.layout;

  const canvasContainerRef = useRef<HTMLDivElement>(null);
  const appRef = useRef<Application | null>(null);

  useEffect(() => {
    const container = canvasContainerRef.current;
    if (!container || !heatmap) return;

    let cancelled = false;

    async function init() {
      if (!container || !heatmap) return;
      // Create the app if it doesn't exist yet
      if (!appRef.current) {
        const app = new Application();
        await app.init({
          preference: "webgl",
          background: 0x1a1a1a,
          width: heatmap.width,
          height: heatmap.height,
        });

        if (cancelled) {
          app.destroy(true);
          return;
        }

        container.appendChild(app.canvas);
        app.canvas.style.width = "100%";
        app.canvas.style.height = "100%";
        appRef.current = app;
      }

      appRef.current.stage.removeChildren();
      appRef.current.stage.addChild(buildHeatmapMesh(heatmap));
    }

    init();

    return () => {
      cancelled = true;
      appRef.current?.destroy(true, { children: true });
      appRef.current = null;
    };
  }, [heatmap]);

  const canvasWidth = 1400;
  const canvasHeight = 700;

  const width = heatmap ? heatmap.width : 1;
  const height = heatmap ? heatmap.height : 1;

  const { scale, offsetX, offsetY } = GetScaleAndOffset(
    layout,
    canvasWidth,
    canvasHeight,
  );

  const { heatmapScale, heatmapOffsetX, heatmapOffsetY } =
    GetHeatmapScaleAndOffset(layout, canvasWidth, canvasHeight, width, height);

  return (
    <div className={styles["editor-container"]}>
      <nav className={styles["editor-nav"]}>
        <button
          className={styles["back-btn"]}
          onClick={() =>
            getHeatmap(state.clientId, { startStep: 0, endStep: 5000 })
          }
        >
          Get Heatmap
        </button>

        <div className={styles["editor-status"]}>
          Selected: <strong>{state.config.layout.name ?? "Custom"}</strong>
        </div>
      </nav>

      <main className={styles["canvas-frame"]}>
        <App width={canvasWidth} height={canvasHeight} background={0x1a1a1a}>
          <container scale={{ x: scale, y: -scale }} x={offsetX} y={offsetY}>
            <DrawLayout layout={layout} />
          </container>
          {heatmap && layout !== emptyLayout && (
            <HeatmapLayer
              heatmap={heatmap}
              scale={heatmapScale}
              offsetX={heatmapOffsetX}
              offsetY={heatmapOffsetY}
            />
          )}
        </App>
      </main>
    </div>
  );
}

export function GetHeatmapScaleAndOffset(
  layout: Layout,
  canvasWidth: number,
  canvasHeight: number,
  width: number,
  height: number,
) {
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;

  const processPoint = ([x, y]: Point) => {
    if (x < minX) minX = x;
    if (y < minY) minY = y;
    if (x > maxX) maxX = x;
    if (y > maxY) maxY = y;
  };

  layout.positive.forEach(processPoint);

  const worldWidth = maxX - minX;
  const worldHeight = maxY - minY;

  const scaleX = (0.9 * canvasWidth) / width;
  const scaleY = (0.9 * canvasHeight) / height;
  const scale = Math.min(scaleX, scaleY);

  const offsetX = (canvasWidth - scale * width) / 2;
  const offsetY = (canvasHeight - scale * height) / 2;

  return {
    heatmapScale: scale,
    heatmapOffsetX: offsetX,
    heatmapOffsetY: offsetY,
  };
}
