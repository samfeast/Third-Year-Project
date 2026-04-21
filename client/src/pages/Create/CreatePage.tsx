import { useEffect, useState } from "react";
import { useStore } from "../../store/StoreProvider";
import type { Layout } from "../../features/layout/types";

import { Application, extend } from "@pixi/react";
import { Container, Graphics } from "pixi.js";

import ToggleSwitch from "../../components/ToggleSwitch/ToggleSwitch";
import PresetCard from "./PresetCard";

import { preset1 } from "../../features/layout/presets/preset1";
import { preset2 } from "../../features/layout/presets/preset2";
import { preset3 } from "../../features/layout/presets/preset3";
import { preset4 } from "../../features/layout/presets/preset4";
import { preset5 } from "../../features/layout/presets/preset5";
import { emptyLayout } from "../../features/layout/defaults";

import preset1Img from "../../features/layout/presets/assets/preset1Img.png";
import preset2Img from "../../features/layout/presets/assets/preset2Img.png";
import preset3Img from "../../features/layout/presets/assets/preset3Img.png";
import preset4Img from "../../features/layout/presets/assets/preset4Img.png";
import customImg from "../../features/layout/presets/assets/custom.png";

import styles from "./CreatePage.module.css";
import { validateLayout } from "../../utils/ValidateLayout";

import { snapshotStore } from "../../features/simulation/snapshotStore";
import { heatmapStore } from "../../features/analysis/heatMapStore";
import { GetScaleAndOffset } from "../Simulate/SimulationCanvas";
import DrawLayout from "../Simulate/DrawLayout";

export default function CreatePage() {
  const { state, dispatch } = useStore();

  const [layout, setLayout] = useState<Layout | null>(
    state.config.layout === emptyLayout ? null : state.config.layout,
  );

  function handlePresetSelect(name: string) {
    let layout: Layout;
    switch (name) {
      case "Preset 1":
        layout = preset1;
        break;
      case "Preset 2":
        layout = preset2;
        break;

      case "Preset 3":
        layout = preset3;
        break;
      case "Preset 4":
        layout = preset4;
        break;
      case "Preset 5":
        layout = preset5;
        break;
      default:
        layout = emptyLayout;
    }
    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: state.config.speedScale,
        exitRadius: state.config.exitRadius,
        layout: layout,
      },
    });

    setLayout(layout);

    snapshotStore.clearSnapshotBuffer();
    heatmapStore.setHeatmap(null);
  }

  function handleUpload(setLayout: (layout: Layout) => void) {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "application/json";

    input.onchange = async (event: Event) => {
      const file = (event.target as HTMLInputElement).files?.[0];
      if (!file) return;

      try {
        const text = await file.text();
        const json = JSON.parse(text);

        const layout = validateLayout(json);
        dispatch({
          type: "SET_CONFIG",
          payload: {
            agentDensity: state.config.agentDensity,
            agentStartPositions: state.config.agentStartPositions,
            agentRadius: state.config.agentRadius,
            seed: state.config.seed,
            speedShape: state.config.speedShape,
            speedScale: state.config.speedScale,
            exitRadius: state.config.exitRadius,
            layout: layout,
          },
        });

        setLayout(layout);

        snapshotStore.clearSnapshotBuffer();
        heatmapStore.setHeatmap(null);
      } catch (err) {
        console.error("Failed to load layout:", err);
        alert("Invalid layout file");
      }
    };

    input.click();
  }

  function handleDiscardSelectedLayout() {
    setLayout(null);

    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: state.config.agentDensity,
        agentStartPositions: state.config.agentStartPositions,
        agentRadius: state.config.agentRadius,
        seed: state.config.seed,
        speedShape: state.config.speedShape,
        speedScale: state.config.speedScale,
        exitRadius: state.config.exitRadius,
        layout: emptyLayout,
      },
    });
  }

  const presets = [
    { name: "Preset 1", description: "Exit Points: 1", img: preset1Img },
    { name: "Preset 2", description: "Exit Points: 1", img: preset2Img },
    {
      name: "Preset 3",
      description: "Exit Points: 1",
      img: preset3Img,
    },
    { name: "Preset 4", description: "Exit Points: 2", img: preset4Img },
    { name: "Preset 5", description: "Exit Points: 1", img: preset4Img },
  ];

  const canvasWidth = 1400;
  const canvasHeight = 700;

  const { scale, offsetX, offsetY } = GetScaleAndOffset(
    layout ?? emptyLayout,
    canvasWidth,
    canvasHeight,
  );

  return (
    <div>
      {layout ? (
        <div className={styles["editor-container"]}>
          <nav className={styles["editor-nav"]}>
            <button
              className={styles["back-btn"]}
              onClick={handleDiscardSelectedLayout}
            >
              Discard & Change Layout
            </button>
            <div className={styles["editor-status"]}>
              Selected: <strong>{state.config.layout.name ?? "Custom"}</strong>
            </div>
          </nav>

          <main className={styles["canvas-frame"]}>
            <Application
              width={canvasWidth}
              height={canvasHeight}
              background={0x1a1a1a}
            >
              <container
                scale={{ x: scale, y: -scale }}
                x={offsetX}
                y={offsetY}
              >
                <DrawLayout layout={layout} />
              </container>
            </Application>
          </main>
        </div>
      ) : (
        <>
          <h2 className={styles["main-title"]}>Select Or Upload Layout</h2>
          <div className={styles["preset-grid"]}>
            <PresetCard
              name={"Custom"}
              imageSrc={customImg}
              description={
                "Upload your own custom layout\nSee guide page for format specification"
              }
              onSelect={() => handleUpload(setLayout)}
              upload={true}
            />
            {presets.map((preset) => (
              <PresetCard
                key={preset.name}
                name={preset.name}
                imageSrc={preset.img}
                description={preset.description}
                onSelect={handlePresetSelect}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
